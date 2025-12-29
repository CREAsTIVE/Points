using Points.Windows.SignalCreation.Presets;

public class SawtoothWave : IGenerationPreset {
	public string Name => "Пилообразный сигнал";

	IGenerationPreset.Parameter<float> frequency = new(1, "Частота") {
		Validator = (v) => v >= float.Epsilon
	};

	IGenerationPreset.Parameter<float> phase = new(0, "Фаза");

	IGenerationPreset.Parameter<float> amplitude = new(1, "Амплитуда") {
		Validator = (v) => v >= float.Epsilon
	};

	// IGenerationPreset.Parameter<bool> reverse = new(false, "Обратная пила");

	public IEnumerable<IGenerationPreset.IParameter> Parameters => new List<IGenerationPreset.IParameter>()
	{
		frequency, phase, amplitude // , reverse
	};

	public async IAsyncEnumerable<float> GetPoints(float timeStep, int amount) {
		for (int ix = 0; ix < amount; ix++) {
			float x = ix * timeStep;
			float period = 1.0f / frequency.Value;
			float normalizedTime = (x - phase.Value) % period;
			if (normalizedTime < 0) normalizedTime += period;

			float t = normalizedTime / period;

			if (/*reverse.Value*/ true) {
				// Обратная пила: от max к min
				yield return amplitude.Value * (1.0f - 2.0f * t);
			} else {
				// Прямая пила: от min к max
				yield return amplitude.Value * (2.0f * t - 1.0f);
			}
		}
	}
}