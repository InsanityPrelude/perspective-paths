using RimWorld;
using UnityEngine;
using Verse;

namespace PerspectivePaths
{
    public class Designator_PP : Designator_Cells
    {    
        protected Area selectedArea;
        protected string areaLabel;

        public Designator_PP(DesignateMode mode)
        {
            this.soundDragSustain = SoundDefOf.Designate_DragStandard;
            this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            this.useMouseIcon = true;
        }
        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            if (!loc.InBounds(base.Map))
            {
                return false;
            }
            return true;
        }
        public override void ProcessInput(Event ev)
        {
            if (!base.CheckCanInteract()) return;

            selectedArea = Map.areaManager.GetLabeled(areaLabel);
            if (selectedArea == null) Map.areaManager.areas.Add(new Area_InvertEdges(base.Map.areaManager, areaLabel));
            else base.ProcessInput(ev);
        }
        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
            selectedArea?.MarkForDraw();
        }
        public override void FinalizeDesignationSucceeded()
        {
            base.FinalizeDesignationSucceeded();
        }
        public override int DraggableDimensions
		{
			get { return 2; }
		}
		public override bool DragDrawMeasurements
		{
			get { return true; }
		}
    }
    class Designator_InvertEdgesClear : Designator_PP
    {
        public Designator_InvertEdgesClear() : base(DesignateMode.Remove)
        {
            defaultLabel = "Icon.InvertEdgesClear.Label".Translate();
            defaultDesc = "Icon.InvertEdgesClear.Desc".Translate();
            icon = Mod_PerspectivePaths.iconHardEdgedAreaClear;
            areaLabel = Mod_PerspectivePaths.areaName;
        }
        public override void DesignateSingleCell(IntVec3 c)
        {
            selectedArea[c] = false;
            Find.CurrentMap.mapDrawer.SectionAt(c).dirtyFlags = MapMeshFlagDefOf.Terrain;
        }
        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            return c.InBounds(Map) && selectedArea != null && selectedArea[c];
        }
    }
    class Designator_InvertEdgesExpand : Designator_PP
    {
        public Designator_InvertEdgesExpand() : base(DesignateMode.Add)
        {
            defaultLabel = "Icon.InvertEdgesExpand.Label".Translate();
            defaultDesc = "Icon.InvertEdgesExpand.Desc".Translate();
            icon = Mod_PerspectivePaths.iconHardEdgedArea;
            areaLabel = Mod_PerspectivePaths.areaName;
        }
        public override void DesignateSingleCell(IntVec3 c)
        {
            selectedArea[c] = true;
            Find.CurrentMap.mapDrawer.SectionAt(c).dirtyFlags = MapMeshFlagDefOf.Terrain;
        }
        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            return c.InBounds(Map) && selectedArea != null && !selectedArea[c] && !c.Impassable(Map);
        }
    }
}
