using System;
using System.Threading.Tasks;
using UnityEngine;

namespace ModMenuCrew.Web;

public class WebRadarPlugin : MonoBehaviour
{
	public WebRadarPlugin(IntPtr ptr)
		: base(ptr)
	{
	}

	private void OnDestroy()
	{
		Task.Run((Action)WebRadarService.Stop);
	}

	private void OnApplicationQuit()
	{
		Task.Run((Action)WebRadarService.Stop);
	}
}
