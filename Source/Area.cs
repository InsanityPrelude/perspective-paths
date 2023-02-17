using Verse;
using System;
using UnityEngine;

namespace PerspectivePaths
{   
	public class Area_InvertEdges : Area
	{
		String label;
		public Area_InvertEdges() { }
		public Area_InvertEdges(AreaManager areaManager, string label) : base(areaManager)
        {
            this.label = label;
        }
		public override string Label
		{
			get
			{
				return Mod_PerspectivePaths.areaName;
			}
		}
		public override Color Color
		{
			get
			{
				return new Color(0.3f, 0.3f, 0.6f, 0.4f);
			}
		}

		public override int ListPriority
		{
			get
			{
				return 200;
			}
		}
		public override string GetUniqueLoadID()
		{
			return ID + Mod_PerspectivePaths.areaName;
		}
	}
}