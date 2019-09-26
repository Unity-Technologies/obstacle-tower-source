﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Barracuda;
using MLAgents.InferenceBrain;
using UnityEngine.Profiling;

namespace MLAgents
{
    public enum InferenceDevice
    {
        CPU = 0,
        GPU = 1
    }
    
    /// <summary>
    /// The Learning Brain works differently if you are training it or not.
    /// When training your Agents, drag the Learning Brain to the Academy's BroadcastHub and check
    /// the checkbox Control. When using a pretrained model, just drag the Model file into the
    /// Model property of the Learning Brain.
    /// The property model corresponds to the Model currently attached to the Brain. Before
    /// being used, a call to ReloadModel is required.
    /// When the Learning Brain is not training, it uses a TensorFlow model to make decisions.
    /// The Proximal Policy Optimization (PPO) and Behavioral Cloning algorithms included with
    /// the ML-Agents SDK produce trained TensorFlow models that you can use with the
    /// Learning Brain.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLearningBrain", menuName = "ML-Agents/Learning Brain")]
    public class LearningBrain : Brain
    {
        private ITensorAllocator _tensorAllocator;
        private TensorGenerator _tensorGenerator;
        private TensorApplier _tensorApplier;
#if ENABLE_TENSORFLOW
        public TextAsset model;
        private ModelParamLoader _modelParamLoader;
        private TFSharpInferenceEngine _engine;
#else 
        public NNModel model;
        private Model _barracudaModel;
        private IWorker _engine;
        private bool _verbose = false;
        
        private BarracudaModelParamLoader _modelParamLoader;
        private string[] _outputNames;
#endif
        
        [Tooltip("Inference execution device. CPU is the fastest option for most of ML Agents models. " +
                 "(This field is not applicable for training).")]
        public InferenceDevice inferenceDevice = InferenceDevice.CPU;
        
        private IReadOnlyList<TensorProxy> _inferenceInputs;
        private IReadOnlyList<TensorProxy> _inferenceOutputs;

        [NonSerialized]
        private bool _isControlled;

        /// <summary>
        /// When Called, the brain will be controlled externally. It will not use the
        /// model to decide on actions.
        /// </summary>
        public void SetToControlledExternally()
        {
            _isControlled = true;
        }
        
        /// <inheritdoc />
        protected override void Initialize()
        {
            ReloadModel();
        }
        
        /// <summary>
        /// Initializes the Brain with the Model that it will use when selecting actions for
        /// the agents
        /// </summary>
        /// <param name="seed"> The seed that will be used to initialize the RandomNormal
        /// and Multinomial obsjects used when running inference.</param>
        /// <exception cref="UnityAgentsException">Throws an error when the model is null
        /// </exception>
        public void ReloadModel(int seed = 0)
        {
            if (_tensorAllocator == null)
                _tensorAllocator = new TensorCachingAllocator();
            
#if ENABLE_TENSORFLOW
            if (model != null)
            {
                _engine = new TFSharpInferenceEngine();
                _engine.PrepareModel(model.bytes);
            }
            else
            {
                _engine = null;
            }
            _modelParamLoader = ModelParamLoader.GetLoaderAndCheck(_engine, brainParameters);
            _inferenceInputs = _modelParamLoader.GetInputTensors();
            _inferenceOutputs = _modelParamLoader.GetOutputTensors();
            _tensorGenerator = new TensorGenerator(brainParameters, seed, _tensorAllocator);
            _tensorApplier = new TensorApplier(brainParameters, seed, _tensorAllocator);
#else
            if (model != null)
            {
                #if BARRACUDA_VERBOSE
                _verbose = true;
                #endif

                D.logEnabled = _verbose;
                
                // Cleanup previous instance
                if (_engine != null)
                    _engine.Dispose();
                
                _barracudaModel = ModelLoader.Load(model.Value);
                var executionDevice = inferenceDevice == InferenceDevice.GPU
                    ? BarracudaWorkerFactory.Type.ComputePrecompiled
                    : BarracudaWorkerFactory.Type.CSharp;
                                       
                _engine = BarracudaWorkerFactory.CreateWorker(executionDevice, _barracudaModel, _verbose);
            }
            else
            {
                _barracudaModel = null;
                _engine = null;
            }

            _modelParamLoader = BarracudaModelParamLoader.GetLoaderAndCheck(_engine, _barracudaModel, brainParameters);
            _inferenceInputs = _modelParamLoader.GetInputTensors();
            _outputNames = _modelParamLoader.GetOutputNames();
            _tensorGenerator = new TensorGenerator(brainParameters, seed, _tensorAllocator, _barracudaModel);
            _tensorApplier = new TensorApplier(brainParameters, seed, _tensorAllocator, _barracudaModel);
#endif
        }
        
        /// <summary>
        /// Return a list of failed checks corresponding to the failed compatibility checks
        /// between the Model and the BrainParameters. Note : This does not reload the model.
        /// If changes have been made to the BrainParameters or the Model, the model must be
        /// reloaded using GiveModel before trying to get the compatibility checks.
        /// </summary>
        /// <returns> The list of the failed compatibility checks between the Model and the
        /// Brain Parameters</returns>
        public IEnumerable<string> GetModelFailedChecks()
        {

#if ENABLE_TENSORFLOW
            return (_modelParamLoader != null) ? _modelParamLoader.GetChecks() : new List<string>();
#else
            return (_modelParamLoader != null) ? _modelParamLoader.GetChecks() : new List<string>();
#endif
        }

        /// <inheritdoc />
        protected override void DecideAction()
        {
            if (_isControlled)
            {
                agentInfos.Clear();
                return;
            }
            var currentBatchSize = agentInfos.Count();
            if (currentBatchSize == 0)
            {
                return;
            }
            
            Profiler.BeginSample("LearningBrain.DecideAction");
            
#if ENABLE_TENSORFLOW
            if (_engine == null)
            {
                Debug.LogError($"No model was present for the Brain {name}.");
                return;
            }
            // Prepare the input tensors to be feed into the engine
            _tensorGenerator.GenerateTensors(_inferenceInputs, currentBatchSize, agentInfos);
            
            // Prepare the output tensors to be feed into the engine
            _tensorGenerator.GenerateTensors(_inferenceOutputs, currentBatchSize, agentInfos);

            // Execute the Model
            Profiler.BeginSample($"MLAgents.{name}.ExecuteGraph");
            _engine.ExecuteGraph(_inferenceInputs, _inferenceOutputs);
            Profiler.EndSample();

            // Update the outputs
            _tensorApplier.ApplyTensors(_inferenceOutputs, agentInfos);
#else
            if (_engine == null)
            {
                Debug.LogError($"No model was present for the Brain {name}.");
                return;
            }
            
            Profiler.BeginSample($"MLAgents.{name}.GenerateTensors");
            // Prepare the input tensors to be feed into the engine
            _tensorGenerator.GenerateTensors(_inferenceInputs, currentBatchSize, agentInfos);
            Profiler.EndSample();

            Profiler.BeginSample($"MLAgents.{name}.PrepareBarracudaInputs");
            var inputs = PrepareBarracudaInputs(_inferenceInputs);
            Profiler.EndSample();

            // Execute the Model
            Profiler.BeginSample($"MLAgents.{name}.ExecuteGraph");
            _engine.Execute(inputs);
            Profiler.EndSample();

            Profiler.BeginSample($"MLAgents.{name}.FetchBarracudaOutputs");
            _inferenceOutputs = FetchBarracudaOutputs(_outputNames);
            Profiler.EndSample();

            Profiler.BeginSample($"MLAgents.{name}.ApplyTensors");
            // Update the outputs
            _tensorApplier.ApplyTensors(_inferenceOutputs, agentInfos);
            Profiler.EndSample();
#endif
            agentInfos.Clear();
            Profiler.EndSample();
        }
        
#if !ENABLE_TENSORFLOW
        protected Dictionary<string, Tensor> PrepareBarracudaInputs(IEnumerable<TensorProxy> infInputs)
        {
            var inputs = new Dictionary<string, Tensor>();
            foreach (var inp in _inferenceInputs)
            {        
                inputs[inp.Name] = inp.Data;
            }

            return inputs;
        }

        protected List<TensorProxy> FetchBarracudaOutputs(string[] names)
        {
            var outputs = new List<TensorProxy>();
            foreach (var name in names)
            {
                var outp = _engine.Peek(name);
                outputs.Add(TensorUtils.TensorProxyFromBarracuda(outp, name));
            }

            return outputs;
        }
#endif
        
        public void OnDisable()
        {
#if !ENABLE_TENSORFLOW
            _engine?.Dispose();
#endif
            _tensorAllocator?.Reset(false);
        }

    }
}
