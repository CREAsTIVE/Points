using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Points.Windows.SignalCreation.Presets; 
/// <summary>
/// Base interface for signal generator
/// </summary>
public interface IGenerationPreset {
	public static List<IGenerationPreset> Common { get; } = new() {
		new Sin(),
		new RandomWalker(),
		new Line()
	};

	public string Name { get; }
	IEnumerable<IParameter> Parameters { get; }
	IAsyncEnumerable<float> GetPoints(float timeStep, int amount);

	public interface IParameter {
		public FrameworkElement Content { get; }
		/// <summary>
		/// Set value, that user will input in a Content
		/// </summary>
		/// <param name="value"></param>
		public void SetBindedValue(object? value);
		public string Name { get; }
	}

	public class Parameter<T> : IParameter {
		static Regex onlyNumbers = new(@"^-?[0-9]*\.?[0-9]*$");// new Regex(@"^-?[0-9]+(\.[0-9]+)?$|^$");

		Dictionary<Type, Func<IParameter, FrameworkElement>> elementFabric = new() {
			{ typeof(float), 
				parameter => {
					static float GetValue(string text) {
						if (text == "") { 
							return 0; 
						}
						if (float.TryParse(text, CultureInfo.InvariantCulture, out var val)) {
							return val; // TODO: Show red box
						}

						return 0;
					} 
					
					var element = new TextBox();

					element.PreviewTextInput += (obj, arg) => {
						arg.Handled = !onlyNumbers.IsMatch(arg.Text);
					};

					element.TextChanged += (obj, arg) => parameter.SetBindedValue(GetValue(element.Text));
					return element;
				}
			}
		};
		public Func<T, bool>? Validator = null; // TODO: Implement
		public FrameworkElement Content { get; }

		public T Value;
		string name;
		public Parameter(T initial, string name) {
			Value = initial;
			this.name = name;
			Content = elementFabric[typeof(T)](this);
		}

		public string Name => name;

		public void SetBindedValue(object? value) {
			Value = (T)value!;
		}
	}
}
