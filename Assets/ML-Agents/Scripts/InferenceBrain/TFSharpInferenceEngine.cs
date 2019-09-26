﻿#if ENABLE_TENSORFLOW
using System.Collections.Generic;
using TensorFlow;
using System.Linq;
using System;
using UnityEngine.Profiling;
using System.Runtime.InteropServices;
using Barracuda;
using UnityEngine;

namespace MLAgents.InferenceBrain
{
    /// <summary>
    /// TFSharpInferenceEngine - Inference engine utilizing the TensorFlow Sharp package to run inference
    /// on frozen TensorFlow models
    /// </summary>
    public class TFSharpInferenceEngine
    {
        private TFGraph m_graph;
        private TFSession m_session;

        public void PrepareModel(byte[] model)
        {
            Profiler.BeginSample("TFSharpInferenceComponent.PrepareModel");

#if UNITY_ANDROID && !UNITY_EDITOR
            // This needs to ba called only once and will raise an exception if called multiple times 
            try{
                TensorFlowSharp.Android.NativeBinding.Init();
            }
            catch{

            }
#endif
            m_graph = new TFGraph();
            m_graph.Import(model);
            m_session = new TFSession(m_graph);
            Profiler.EndSample();
        }

        public int ExecuteGraph(IEnumerable<TensorProxy> inputs_it, IEnumerable<TensorProxy> outputs_it)
        {
            Profiler.BeginSample("TFSharpInferenceComponent.ExecuteGraph");
            TensorProxy[] inputs = inputs_it.ToArray();
            TensorProxy[] outputs = outputs_it.ToArray();

            // TODO: Can/should we pre-allocate that?
            TFSession.Runner runner = m_session.GetRunner();

            inputs.ToList().ForEach((TensorProxy input) =>
            {   
                if (input.Shape.Length == 0)
                {
                    var data = input.Data[0];
                    if (input.DataType == typeof(int))
                    {
                        runner.AddInput(m_graph[input.Name][0], (int)data);
                    }
                    else
                    {
                        runner.AddInput(m_graph[input.Name][0], (float)data);
                    }
                }
                else
                {
                    runner.AddInput(m_graph[input.Name][0], input.DataType == typeof(int) ?
                                                            TensorUtils.BarracudaToIntArray(input.Data) :
                                                            TensorUtils.BarracudaToFloatArray(input.Data));
                }
            });

            // TODO: better way to pre-allocate this?
            outputs.ToList().ForEach(s => runner.Fetch(s.Name));

            TFStatus status = new TFStatus();
            Profiler.BeginSample("TFSharpInferenceComponent.ExecuteGraph.RunnerRun");
            var out_tensors = runner.Run(status);
            Profiler.EndSample();

            if (!status.Ok)
            {
                Debug.LogError(status.StatusMessage);
                return -1;
            }

            Debug.Assert(outputs.Length == out_tensors.Length);

            for (var i = 0; i < outputs.Length; ++i)
            {
                if (outputs[i].Shape.Length == 0)
                {
                    // Handle scalars
                    outputs[i].Data = new Tensor(1,1);
                    outputs[i].Data[0] = (float)(int)out_tensors[i].GetValue();
                }
                else
                {
                    outputs[i].Data = TensorUtils.ArrayToBarracuda(out_tensors[i].GetValue() as Array);
                }
            }

            Profiler.EndSample();
            // TODO: create error codes
            return 0;
        }

        [DllImport("libtensorflow")]
        private static extern unsafe void TF_OperationGetAttrType(IntPtr oper, string attr_name, 
            TFDataType* value, IntPtr status);
                
        [DllImport("libtensorflow")]
        private static extern unsafe void TF_OperationGetAttrShape(IntPtr oper, string attr_name, long[] value, 
            int num_dims, IntPtr status);

        private TensorProxy GetOpMetadata(TFOperation op)
        {
            TFStatus status = new TFStatus();
                        
            // Query the shape
            long[] shape = null;
            var shape_attr = op.GetAttributeMetadata("shape", status);
            if (!status.Ok || shape_attr.TotalSize <= 0)
            {
                Debug.LogWarning($"Operation {op.Name} does not contain shape attribute or it" +
                                 $" doesn't contain valid shape data! Status: {status.StatusMessage}");
            }
            else
            {
                if (shape_attr.IsList)
                {
                    throw new NotImplementedException("Querying lists is not implemented yet!");
                }
                else
                {
                    TFStatus s = new TFStatus();
                    long[] dims = new long[shape_attr.TotalSize];
                    TF_OperationGetAttrShape(op.Handle, "shape", dims, (int)shape_attr.TotalSize, 
                        s.Handle);
                    if (!status.Ok)
                    {
                        throw new FormatException("Could not query model for op shape (" + op.Name + ")");
                    }
                    else
                    {
                        shape = new long[dims.Length];
                        for (int i = 0; i < shape_attr.TotalSize; ++i)
                        {
                            if (dims[i] == -1)
                            {
                                // we have to use batchsize 1
                                shape[i] = 1;
                            }
                            else
                            {
                                shape[i] = dims[i];
                            }
                        }
                    }
                }
            }
                        
            // Query the data type
            TFDataType type_value = new TFDataType();
            unsafe
            {
                TFStatus s = new TFStatus();
                TF_OperationGetAttrType(op.Handle, "dtype", &type_value, s.Handle);
                if (!s.Ok)
                {
                    Debug.LogWarning("Operation " + op.Name + 
                                     ": error retrieving dtype, assuming float!");
                    type_value = TFDataType.Float;
                }
            }

            TensorProxy.TensorType placeholder_type = TensorProxy.TensorType.FloatingPoint;
            switch (type_value)
            {
                case TFDataType.Float:
                    placeholder_type = TensorProxy.TensorType.FloatingPoint;
                    break;
                case TFDataType.Int32:
                    placeholder_type = TensorProxy.TensorType.Integer;
                    break;
                default:
                    Debug.LogWarning("Operation " + op.Name + 
                                     " is not a float/integer. Proceed at your own risk!");
                    break;
            }
                        
            TensorProxy t = new TensorProxy
            {
                Data = null,
                Name = op.Name,
                Shape = shape,
                ValueType = placeholder_type
            };
            return t;
        }

        public IReadOnlyList<TensorProxy> InputFeatures()
        {
            List<TensorProxy> inputs = new List<TensorProxy>();
            foreach (var op in m_graph.GetEnumerator())
            {
                if (op.OpType == "Placeholder")
                {
                    inputs.Add(GetOpMetadata(op));
                }
            }

            return inputs;
        }
    }
}
#endif
