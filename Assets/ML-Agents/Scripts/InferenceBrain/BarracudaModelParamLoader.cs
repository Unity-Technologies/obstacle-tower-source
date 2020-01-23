using System;
using System.Collections.Generic;
using System.Linq;
using Barracuda;
using MLAgents.Sensor;
using UnityEngine;

namespace MLAgents.InferenceBrain
{
    /// <summary>
    /// Prepares the Tensors for the Learning Brain and exposes a list of failed checks if Model
    /// and BrainParameters are incompatible.
    /// </summary>
    public class BarracudaModelParamLoader
    {
        enum ModelActionType
        {
            Unknown,
            Discrete,
            Continuous
        }

        const long k_ApiVersion = 2;

        /// <summary>
        /// Generates the Tensor inputs that are expected to be present in the Model.
        /// </summary>
        /// <param name="model">
        /// The Barracuda engine model for loading static parameters
        /// </param>
        /// <returns>TensorProxy IEnumerable with the expected Tensor inputs</returns>
        public static IReadOnlyList<TensorProxy> GetInputTensors(Model model)
        {
            var tensors = new List<TensorProxy>();

            if (model == null)
                return tensors;

            foreach (var input in model.inputs)
            {
                tensors.Add(new TensorProxy
                {
                    name = input.name,
                    valueType = TensorProxy.TensorType.FloatingPoint,
                    data = null,
                    shape = input.shape.Select(i => (long)i).ToArray()
                });
            }

            foreach (var mem in model.memories)
            {
                tensors.Add(new TensorProxy
                {
                    name = mem.input,
                    valueType = TensorProxy.TensorType.FloatingPoint,
                    data = null,
                    shape = TensorUtils.TensorShapeFromBarracuda(mem.shape)
                });
            }

            tensors.Sort((el1, el2) => el1.name.CompareTo(el2.name));

            return tensors;
        }

        public static int GetNumVisualInputs(Model model)
        {
            var count = 0;
            if (model == null)
                return count;

            foreach (var input in model.inputs)
            {
                if (input.shape.Length == 4)
                {
                    if (input.name.StartsWith(TensorNames.VisualObservationPlaceholderPrefix))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Generates the Tensor outputs that are expected to be present in the Model.
        /// </summary>
        /// <param name="model">
        /// The Barracuda engine model for loading static parameters
        /// </param>
        /// <returns>TensorProxy IEnumerable with the expected Tensor outputs</returns>
        public static string[] GetOutputNames(Model model)
        {
            var names = new List<string>();

            if (model == null)
            {
                return names.ToArray();
            }

            names.Add(TensorNames.ActionOutput);

            var memory = (int)model.GetTensorByName(TensorNames.MemorySize)[0];
            if (memory > 0)
            {
                foreach (var mem in model.memories)
                {
                    names.Add(mem.output);
                }
            }

            names.Sort();

            return names.ToArray();
        }

        /// <summary>
        /// Factory for the ModelParamLoader : Creates a ModelParamLoader and runs the checks
        /// on it.
        /// </summary>
        /// <param name="model">
        /// The Barracuda engine model for loading static parameters
        /// </param>
        /// <param name="brainParameters">
        /// The BrainParameters that are used verify the compatibility with the InferenceEngine
        /// </param>
        /// <param name="sensorComponents">Attached sensor components</param>
        /// <returns>The list the error messages of the checks that failed</returns>
        public static IEnumerable<string> CheckModel(Model model, BrainParameters brainParameters, SensorComponent[] sensorComponents)
        {
            List<string> failedModelChecks = new List<string>();
            if (model == null)
            {
                failedModelChecks.Add(
                    "There is no model for this Brain, cannot run inference. " +
                    "(But can still train)");
                return failedModelChecks;
            }

            var modelApiVersion = (int)model.GetTensorByName(TensorNames.VersionNumber)[0];
            var memorySize = (int)model.GetTensorByName(TensorNames.MemorySize)[0];
            var isContinuousInt = (int)model.GetTensorByName(TensorNames.IsContinuousControl)[0];
            var isContinuous = GetActionType(isContinuousInt);
            var actionSize = (int)model.GetTensorByName(TensorNames.ActionOutputShape)[0];
            if (modelApiVersion == -1)
            {
                failedModelChecks.Add(
                    "Model was not trained using the right version of ML-Agents. " +
                    "Cannot use this model.");
                return failedModelChecks;
            }
            if (modelApiVersion != k_ApiVersion)
            {
                failedModelChecks.Add(
                    $"Version of the trainer the model was trained with ({modelApiVersion}) " +
                    $"is not compatible with the Brain's version ({k_ApiVersion}).");
                return failedModelChecks;
            }

            failedModelChecks.AddRange(
                CheckIntScalarPresenceHelper(new Dictionary<string, int>()
                {
                    {TensorNames.MemorySize, memorySize},
                    {TensorNames.IsContinuousControl, isContinuousInt},
                    {TensorNames.ActionOutputShape, actionSize}
                })
            );
            failedModelChecks.AddRange(
                CheckInputTensorPresence(model, brainParameters, memorySize, isContinuous, sensorComponents)
            );
            failedModelChecks.AddRange(
                CheckOutputTensorPresence(model, memorySize))
            ;
            failedModelChecks.AddRange(
                CheckInputTensorShape(model, brainParameters, sensorComponents)
            );
            failedModelChecks.AddRange(
                CheckOutputTensorShape(model, brainParameters, isContinuous, actionSize)
            );
            return failedModelChecks;
        }

        /// <summary>
        /// Converts the integer value in the model corresponding to the type of control to a
        /// ModelActionType.
        /// </summary>
        /// <param name="isContinuousInt">
        /// The integer value in the model indicating the type of control
        /// </param>
        /// <returns>The equivalent ModelActionType</returns>
        static ModelActionType GetActionType(int isContinuousInt)
        {
            ModelActionType isContinuous;
            switch (isContinuousInt)
            {
                case 0:
                    isContinuous = ModelActionType.Discrete;
                    break;
                case 1:
                    isContinuous = ModelActionType.Continuous;
                    break;
                default:
                    isContinuous = ModelActionType.Unknown;
                    break;
            }
            return isContinuous;
        }

        /// <summary>
        /// Given a Dictionary of node names to int values, create checks if the values have the
        /// invalid value of -1.
        /// </summary>
        /// <param name="requiredScalarFields"> Mapping from node names to int values</param>
        /// <returns>The list the error messages of the checks that failed</returns>
        static IEnumerable<string> CheckIntScalarPresenceHelper(
            Dictionary<string, int> requiredScalarFields)
        {
            var failedModelChecks = new List<string>();
            foreach (var field in requiredScalarFields)
            {
                if (field.Value == -1)
                {
                    failedModelChecks.Add($"Missing node in the model provided : {field.Key}");
                }
            }
            return failedModelChecks;
        }

        /// <summary>
        /// Generates failed checks that correspond to inputs expected by the model that are not
        /// present in the BrainParameters.
        /// </summary>
        /// <param name="model">
        /// The Barracuda engine model for loading static parameters
        /// </param>
        /// <param name="brainParameters">
        /// The BrainParameters that are used verify the compatibility with the InferenceEngine
        /// </param>
        /// <param name="memory">
        /// The memory size that the model is expecting.
        /// </param>
        /// <param name="isContinuous">
        /// Whether the model is expecting continuous or discrete control.
        /// </param>
        /// <param name="sensorComponents">Array of attached sensor components</param>
        /// <returns>
        /// A IEnumerable of string corresponding to the failed input presence checks.
        /// </returns>
        static IEnumerable<string> CheckInputTensorPresence(
            Model model,
            BrainParameters brainParameters,
            int memory,
            ModelActionType isContinuous,
            SensorComponent[] sensorComponents
            )
        {
            var failedModelChecks = new List<string>();
            var tensorsNames = GetInputTensors(model).Select(x => x.name).ToList();

            // If there is no Vector Observation Input but the Brain Parameters expect one.
            if ((brainParameters.vectorObservationSize != 0) &&
                (!tensorsNames.Contains(TensorNames.VectorObservationPlacholder)))
            {
                failedModelChecks.Add(
                    "The model does not contain a Vector Observation  Placeholder Input. " +
                    "You must set the Vector Observation Space Size to 0.");
            }

            // If there are not enough Visual Observation Input compared to what the
            // sensors expect.
            var visObsIndex = 0;
            for (var sensorIndex = 0; sensorIndex < sensorComponents.Length; sensorIndex++)
            {
                var sensor = sensorComponents[sensorIndex];
                if (!sensor.IsVisual())
                {
                    continue;
                }
                if (!tensorsNames.Contains(
                    TensorNames.VisualObservationPlaceholderPrefix + visObsIndex))
                {
                    failedModelChecks.Add(
                        "The model does not contain a Visual Observation Placeholder Input " +
                        $"for sensor component {visObsIndex} ({sensor.GetType().Name}).");
                }

                visObsIndex++;
            }

            var expectedVisualObs = GetNumVisualInputs(model);
            // Check if there's not enough visual sensors (too many would be handled above)
            if (expectedVisualObs > visObsIndex)
            {
                failedModelChecks.Add(
                    $"The model expects {expectedVisualObs} visual inputs," +
                    $" but only found {visObsIndex} visual sensors."
                );
            }

            // If the model has a non-negative memory size but requires a recurrent input
            if (memory > 0)
            {
                if (!tensorsNames.Any(x => x.EndsWith("_h")) ||
                    !tensorsNames.Any(x => x.EndsWith("_c")))
                {
                    failedModelChecks.Add(
                        "The model does not contain a Recurrent Input Node but has memory_size.");
                }
            }

            // If the model uses discrete control but does not have an input for action masks
            if (isContinuous == ModelActionType.Discrete)
            {
                if (!tensorsNames.Contains(TensorNames.ActionMaskPlaceholder))
                {
                    failedModelChecks.Add(
                        "The model does not contain an Action Mask but is using Discrete Control.");
                }
            }
            return failedModelChecks;
        }

        /// <summary>
        /// Generates failed checks that correspond to outputs expected by the model that are not
        /// present in the BrainParameters.
        /// </summary>
        /// <param name="model">
        /// The Barracuda engine model for loading static parameters
        /// </param>
        /// <param name="memory">The memory size that the model is expecting/</param>
        /// <returns>
        /// A IEnumerable of string corresponding to the failed output presence checks.
        /// </returns>
        static IEnumerable<string> CheckOutputTensorPresence(Model model, int memory)
        {
            var failedModelChecks = new List<string>();
            // If there is no Action Output.
            if (!model.outputs.Contains(TensorNames.ActionOutput))
            {
                failedModelChecks.Add("The model does not contain an Action Output Node.");
            }

            // If there is no Recurrent Output but the model is Recurrent.
            if (memory > 0)
            {
                var memOutputs = model.memories.Select(x => x.output).ToList();

                if (!memOutputs.Any(x => x.EndsWith("_h")) ||
                    !memOutputs.Any(x => x.EndsWith("_c")))
                {
                    failedModelChecks.Add(
                        "The model does not contain a Recurrent Output Node but has memory_size.");
                }
            }
            return failedModelChecks;
        }

        /// <summary>
        /// Checks that the shape of the visual observation input placeholder is the same as the corresponding sensor.
        /// </summary>
        /// <param name="tensorProxy">The tensor that is expected by the model</param>
        /// <param name="sensorComponent">The sensor that produces the visual observation.</param>
        /// <returns>
        /// If the Check failed, returns a string containing information about why the
        /// check failed. If the check passed, returns null.
        /// </returns>
        static string CheckVisualObsShape(
            TensorProxy tensorProxy, SensorComponent sensorComponent)
        {
            var shape = sensorComponent.GetObservationShape();
            var heightBp = shape[0];
            var widthBp = shape[1];
            var pixelBp = shape[2];
            var heightT = tensorProxy.shape[1];
            var widthT = tensorProxy.shape[2];
            var pixelT = tensorProxy.shape[3];
            if ((widthBp != widthT) || (heightBp != heightT) || (pixelBp != pixelT))
            {
                return $"The visual Observation of the model does not match. " +
                    $"Received TensorProxy of shape [?x{widthBp}x{heightBp}x{pixelBp}] but " +
                    $"was expecting [?x{widthT}x{heightT}x{pixelT}].";
            }
            return null;
        }

        /// <summary>
        /// Generates failed checks that correspond to inputs shapes incompatibilities between
        /// the model and the BrainParameters.
        /// </summary>
        /// <param name="model">
        /// The Barracuda engine model for loading static parameters
        /// </param>
        /// <param name="brainParameters">
        /// The BrainParameters that are used verify the compatibility with the InferenceEngine
        /// </param>
        /// <param name="sensorComponents">Attached sensors</param>
        /// <returns>The list the error messages of the checks that failed</returns>
        static IEnumerable<string> CheckInputTensorShape(
            Model model, BrainParameters brainParameters, SensorComponent[] sensorComponents)
        {
            var failedModelChecks = new List<string>();
            var tensorTester =
                new Dictionary<string, Func<BrainParameters, TensorProxy, SensorComponent[], string>>()
            {
                {TensorNames.VectorObservationPlacholder, CheckVectorObsShape},
                {TensorNames.PreviousActionPlaceholder, CheckPreviousActionShape},
                {TensorNames.RandomNormalEpsilonPlaceholder, ((bp, tensor, scs) => null)},
                {TensorNames.ActionMaskPlaceholder, ((bp, tensor, scs) => null)},
                {TensorNames.SequenceLengthPlaceholder, ((bp, tensor, scs) => null)},
                {TensorNames.RecurrentInPlaceholder, ((bp, tensor, scs) => null)},
            };

            foreach (var mem in model.memories)
            {
                tensorTester[mem.input] = ((bp, tensor, scs) => null);
            }

            var visObsIndex = 0;
            for (var sensorIndex = 0; sensorIndex < sensorComponents.Length; sensorIndex++)
            {
                var sensorComponent = sensorComponents[sensorIndex];
                if (!sensorComponent.IsVisual())
                {
                    continue;
                }
                tensorTester[TensorNames.VisualObservationPlaceholderPrefix + visObsIndex] =
                    (bp, tensor, scs) => CheckVisualObsShape(tensor, sensorComponent);
                visObsIndex++;
            }

            // If the model expects an input but it is not in this list
            foreach (var tensor in GetInputTensors(model))
            {
                if (!tensorTester.ContainsKey(tensor.name))
                {
                    if (!tensor.name.Contains("visual_observation"))
                    {
                        failedModelChecks.Add(
                            "Model requires an unknown input named : " + tensor.name);
                    }
                }
                else
                {
                    var tester = tensorTester[tensor.name];
                    var error = tester.Invoke(brainParameters, tensor, sensorComponents);
                    if (error != null)
                    {
                        failedModelChecks.Add(error);
                    }
                }
            }
            return failedModelChecks;
        }

        /// <summary>
        /// Checks that the shape of the Vector Observation input placeholder is the same in the
        /// model and in the Brain Parameters.
        /// </summary>
        /// <param name="brainParameters">
        /// The BrainParameters that are used verify the compatibility with the InferenceEngine
        /// </param>
        /// <param name="tensorProxy">The tensor that is expected by the model</param>
        /// <param name="sensorComponents">Array of attached sensor components</param>
        /// <returns>
        /// If the Check failed, returns a string containing information about why the
        /// check failed. If the check passed, returns null.
        /// </returns>
        static string CheckVectorObsShape(
            BrainParameters brainParameters, TensorProxy tensorProxy, SensorComponent[] sensorComponents)
        {
            var vecObsSizeBp = brainParameters.vectorObservationSize;
            var numStackedVector = brainParameters.numStackedVectorObservations;
            var totalVecObsSizeT = tensorProxy.shape[tensorProxy.shape.Length - 1];

            var totalVectorSensorSize = 0;
            foreach (var sensorComp in sensorComponents)
            {
                if (sensorComp.IsVector())
                {
                    totalVectorSensorSize += sensorComp.GetObservationShape()[0];
                }
            }

            if (vecObsSizeBp * numStackedVector + totalVectorSensorSize != totalVecObsSizeT)
            {
                var sensorSizes = "";
                foreach (var sensorComp in sensorComponents)
                {
                    if (sensorComp.IsVector())
                    {
                        var vecSize = sensorComp.GetObservationShape()[0];
                        if (sensorSizes.Length == 0)
                        {
                            sensorSizes = $"[{vecSize}";
                        }
                        else
                        {
                            sensorSizes += $", {vecSize}";
                        }
                    }
                }

                sensorSizes += "]";
                return $"Vector Observation Size of the model does not match. Was expecting {totalVecObsSizeT} " +
                    $"but received {vecObsSizeBp} x {numStackedVector} vector observations and " +
                    $"SensorComponent sizes: {sensorSizes}.";
            }
            return null;
        }

        /// <summary>
        /// Checks that the shape of the Previous Vector Action input placeholder is the same in the
        /// model and in the Brain Parameters.
        /// </summary>
        /// <param name="brainParameters">
        /// The BrainParameters that are used verify the compatibility with the InferenceEngine
        /// </param>
        /// <param name="tensorProxy"> The tensor that is expected by the model</param>
        /// <param name="sensorComponents">Array of attached sensor components</param>
        /// <returns>If the Check failed, returns a string containing information about why the
        /// check failed. If the check passed, returns null.</returns>
        static string CheckPreviousActionShape(
            BrainParameters brainParameters, TensorProxy tensorProxy, SensorComponent[] sensorComponents)
        {
            var numberActionsBp = brainParameters.vectorActionSize.Length;
            var numberActionsT = tensorProxy.shape[tensorProxy.shape.Length - 1];
            if (numberActionsBp != numberActionsT)
            {
                return "Previous Action Size of the model does not match. " +
                    $"Received {numberActionsBp} but was expecting {numberActionsT}.";
            }
            return null;
        }

        /// <summary>
        /// Generates failed checks that correspond to output shapes incompatibilities between
        /// the model and the BrainParameters.
        /// </summary>
        /// <param name="model">
        /// The Barracuda engine model for loading static parameters
        /// </param>
        /// <param name="brainParameters">
        /// The BrainParameters that are used verify the compatibility with the InferenceEngine
        /// </param>
        /// <param name="isContinuous">
        /// Whether the model is expecting continuous or discrete control.
        /// </param>
        /// <param name="modelActionSize">
        /// The size of the action output that is expected by the model.
        /// </param>
        /// <returns>
        /// A IEnumerable of string corresponding to the incompatible shapes between model
        /// and BrainParameters.
        /// </returns>
        static IEnumerable<string> CheckOutputTensorShape(
            Model model,
            BrainParameters brainParameters,
            ModelActionType isContinuous,
            int modelActionSize)
        {
            var failedModelChecks = new List<string>();
            if (isContinuous == ModelActionType.Unknown)
            {
                failedModelChecks.Add("Cannot infer type of Control from the provided model.");
                return failedModelChecks;
            }
            if (isContinuous == ModelActionType.Continuous &&
                brainParameters.vectorActionSpaceType != SpaceType.Continuous)
            {
                failedModelChecks.Add(
                    "Model has been trained using Continuous Control but the Brain Parameters " +
                    "suggest Discrete Control.");
                return failedModelChecks;
            }
            if (isContinuous == ModelActionType.Discrete &&
                brainParameters.vectorActionSpaceType != SpaceType.Discrete)
            {
                failedModelChecks.Add(
                    "Model has been trained using Discrete Control but the Brain Parameters " +
                    "suggest Continuous Control.");
                return failedModelChecks;
            }
            var tensorTester = new Dictionary<string, Func<BrainParameters, TensorShape, int, string>>();
            if (brainParameters.vectorActionSpaceType == SpaceType.Continuous)
            {
                tensorTester[TensorNames.ActionOutput] = CheckContinuousActionOutputShape;
            }
            else
            {
                tensorTester[TensorNames.ActionOutput] = CheckDiscreteActionOutputShape;
            }
            // If the model expects an output but it is not in this list
            foreach (var name in model.outputs)
            {
                if (tensorTester.ContainsKey(name))
                {
                    var tester = tensorTester[name];
                    var error = tester.Invoke(brainParameters, model.GetShapeByName(name), modelActionSize);
                    if (error != null)
                    {
                        failedModelChecks.Add(error);
                    }
                }
            }
            return failedModelChecks;
        }

        /// <summary>
        /// Checks that the shape of the discrete action output is the same in the
        /// model and in the Brain Parameters.
        /// </summary>
        /// <param name="brainParameters">
        /// The BrainParameters that are used verify the compatibility with the InferenceEngine
        /// </param>
        /// <param name="shape"> The tensor shape that is expected by the model</param>
        /// <param name="modelActionSize">
        /// The size of the action output that is expected by the model.
        /// </param>
        /// <returns>
        /// If the Check failed, returns a string containing information about why the
        /// check failed. If the check passed, returns null.
        /// </returns>
        static string CheckDiscreteActionOutputShape(
            BrainParameters brainParameters, TensorShape shape, int modelActionSize)
        {
            var bpActionSize = brainParameters.vectorActionSize.Sum();
            if (modelActionSize != bpActionSize)
            {
                return "Action Size of the model does not match. The BrainParameters expect " +
                    $"{bpActionSize} but the model contains {modelActionSize}.";
            }
            return null;
        }

        /// <summary>
        /// Checks that the shape of the continuous action output is the same in the
        /// model and in the Brain Parameters.
        /// </summary>
        /// <param name="brainParameters">
        /// The BrainParameters that are used verify the compatibility with the InferenceEngine
        /// </param>
        /// <param name="shape"> The tensor shape that is expected by the model</param>
        /// <param name="modelActionSize">
        /// The size of the action output that is expected by the model.
        /// </param>
        /// <returns>If the Check failed, returns a string containing information about why the
        /// check failed. If the check passed, returns null.</returns>
        static string CheckContinuousActionOutputShape(
            BrainParameters brainParameters, TensorShape shape, int modelActionSize)
        {
            var bpActionSize = brainParameters.vectorActionSize[0];
            if (modelActionSize != bpActionSize)
            {
                return "Action Size of the model does not match. The BrainParameters expect " +
                    $"{bpActionSize} but the model contains {modelActionSize}.";
            }
            return null;
        }
    }
}
