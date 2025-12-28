using ScottPlot.Colormaps;
using ScottPlot.DataGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Points.Windows.SignalCreation.Presets;

public class Line : IGenerationPreset {
	public string Name => "Прямая";

	public IEnumerable<IGenerationPreset.IParameter> Parameters => new List<IGenerationPreset.IParameter>() {
		
	};

	public async IAsyncEnumerable<float> GetPoints(float timeStep, int amount) {
		float v = 0;
		for (int i = 0; i < amount; i++) {
			yield return v++;
		}
	}
}
