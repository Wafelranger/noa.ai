from transformers import AutoTokenizer, MistralForCausalLM

model = MistralForCausalLM.from_pretrained("mistralai/Mistral-7B-v0.1")
tokenizer = AutoTokenizer.from_pretrained("mistralai/Mistral-7B-v0.1")

prompt = "Hey, are you conscious? Can you talk to me?"
inputs = tokenizer(prompt, return_tensors="pt")

# Generate
generate_ids = model.generate(inputs.input_ids, max_length=30)
decoded_output = tokenizer.batch_decode(generate_ids.tolist(), skip_special_tokens=True, clean_up_tokenization_spaces=False)[0]
print(decoded_output)
