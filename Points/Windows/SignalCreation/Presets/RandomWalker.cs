using ScottPlot.Colormaps;
using ScottPlot.DataGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Points.Windows.SignalCreation.Presets;

public class RandomWalker : IGenerationPreset {
	public string Name => "Случайная ходьба";

	public IEnumerable<IGenerationPreset.IParameter> Parameters => new List<IGenerationPreset.IParameter>() {
		
	};

	public async IAsyncEnumerable<float> GetPoints(float timeStep, int amount) {
		var walker = new ScottPlot.DataGenerators.RandomWalker();
		for (int i = 0; i < amount; i++) {
			yield return (float)walker.Next();
		}
	}
}
