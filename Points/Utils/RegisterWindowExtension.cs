using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Points.Utils; 
public static class RegisterWindowExtension {
	public static void RegisterWindow<Window>(this ServiceCollection services) where Window : class {
		// (Using Func<T> instead of Transied to resolve factories for popups)
		services.AddTransient<Window>();
		services.AddSingleton<Func<Window>>(sp => () => sp.GetRequiredService<Window>());
	}

	public static void RegisterWindow<Window, ViewModel>(this ServiceCollection services) where Window : class where ViewModel : class {
		RegisterWindow<Window>(services);

		services.AddTransient<ViewModel>();
	}
}
