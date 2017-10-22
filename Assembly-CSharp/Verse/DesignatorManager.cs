using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class DesignatorManager
	{
		private Designator selectedDesignator;

		private DesignationDragger dragger = new DesignationDragger();

		public Designator SelectedDesignator
		{
			get
			{
				return this.selectedDesignator;
			}
		}

		public DesignationDragger Dragger
		{
			get
			{
				return this.dragger;
			}
		}

		public void Select(Designator des)
		{
			this.Deselect();
			this.selectedDesignator = des;
			this.selectedDesignator.Selected();
		}

		public void Deselect()
		{
			if (this.selectedDesignator != null)
			{
				this.selectedDesignator = null;
				this.dragger.EndDrag();
			}
		}

		private bool CheckSelectedDesignatorValid()
		{
			if (this.selectedDesignator == null)
			{
				return false;
			}
			if (!this.selectedDesignator.CanRemainSelected())
			{
				this.Deselect();
				return false;
			}
			return true;
		}

		public void ProcessInputEvents()
		{
			if (this.CheckSelectedDesignatorValid())
			{
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
				{
					if (this.selectedDesignator.DraggableDimensions == 0)
					{
						Designator designator = this.selectedDesignator;
						AcceptanceReport acceptanceReport = this.selectedDesignator.CanDesignateCell(UI.MouseCell());
						if (acceptanceReport.Accepted)
						{
							designator.DesignateSingleCell(UI.MouseCell());
							designator.Finalize(true);
						}
						else
						{
							Messages.Message(acceptanceReport.Reason, MessageSound.Silent);
							this.selectedDesignator.Finalize(false);
						}
					}
					else
					{
						this.dragger.StartDrag();
					}
					Event.current.Use();
				}
				if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
				{
					goto IL_00e8;
				}
				if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
					goto IL_00e8;
				goto IL_011d;
			}
			return;
			IL_00e8:
			SoundDefOf.CancelMode.PlayOneShotOnCamera(null);
			this.Deselect();
			this.dragger.EndDrag();
			Event.current.Use();
			TutorSystem.Notify_Event("ClearDesignatorSelection");
			goto IL_011d;
			IL_011d:
			if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && this.dragger.Dragging)
			{
				this.selectedDesignator.DesignateMultiCell(this.dragger.DragCells);
				this.dragger.EndDrag();
				Event.current.Use();
			}
		}

		public void DesignationManagerOnGUI()
		{
			this.dragger.DraggerOnGUI();
			if (this.CheckSelectedDesignatorValid())
			{
				this.selectedDesignator.DrawMouseAttachments();
			}
		}

		public void DesignatorManagerUpdate()
		{
			this.dragger.DraggerUpdate();
			if (this.CheckSelectedDesignatorValid())
			{
				this.selectedDesignator.SelectedUpdate();
			}
		}
	}
}
