using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Points.Models; 
public interface ISignalMeta {
	public string Name { get; }
	public DateTime CreationDate { get; }
	public double TimeStep { get; }
}
