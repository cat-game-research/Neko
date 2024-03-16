import onnx
from onnx import numpy_helper

model_path = "./RagdollTrainer/Assets/TFModels/Walker-b0a/WalkBeta1.b0a-400k.onnx"

def count_parameters(onnx_model_path):
    model = onnx.load(onnx_model_path)
    total_parameters = 0
    for initializer in model.graph.initializer:
        total_parameters += numpy_helper.to_array(initializer).size
    return total_parameters

num_params = count_parameters(model_path)
print("num_of_params: ", num_params)
