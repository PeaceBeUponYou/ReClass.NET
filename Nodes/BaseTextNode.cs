﻿using System;
using System.Diagnostics.Contracts;
using ReClassNET.UI;
using ReClassNET.Util;

namespace ReClassNET.Nodes
{
	[ContractClass(typeof(BaseTextNodeContract))]
	abstract class BaseTextNode : BaseNode
	{
		public int CharacterCount { get; set; }

		/// <summary>Size of the node in bytes.</summary>
		public override int MemorySize => CharacterCount * CharacterSize;

		public abstract int CharacterSize { get; }

		public override void CopyFromNode(BaseNode node)
		{
			CharacterCount = node.MemorySize / CharacterSize;
		}

		protected int DrawText(ViewInfo view, int x, int y, string type, int length, string text)
		{
			Contract.Requires(view != null);
			Contract.Requires(type != null);
			Contract.Requires(text != null);

			if (IsHidden)
			{
				return DrawHidden(view, x, y);
			}

			AddSelection(view, x, y, view.Font.Height);
			AddDelete(view, x, y);
			AddTypeDrop(view, x, y);

			x += TXOFFSET;
			x = AddIcon(view, x, y, Icons.Text, HotSpot.NoneId, HotSpotType.None);
			x = AddAddressOffset(view, x, y);

			x = AddText(view, x, y, Program.Settings.TypeColor, HotSpot.NoneId, type) + view.Font.Width;
			x = AddText(view, x, y, Program.Settings.NameColor, HotSpot.NameId, Name);
			x = AddText(view, x, y, Program.Settings.IndexColor, HotSpot.NoneId, "[");
			x = AddText(view, x, y, Program.Settings.IndexColor, 0, length.ToString());
			x = AddText(view, x, y, Program.Settings.IndexColor, HotSpot.NoneId, "]") + view.Font.Width;

			x = AddText(view, x, y, Program.Settings.TextColor, HotSpot.NoneId, "= '");
			x = AddText(view, x, y, Program.Settings.TextColor, HotSpot.NoneId, text.LimitLength(150));
			x = AddText(view, x, y, Program.Settings.TextColor, HotSpot.NoneId, "'") + view.Font.Width;

			x = AddComment(view, x, y);

			return y + view.Font.Height;
		}

		public override void Update(HotSpot spot)
		{
			base.Update(spot);

			if (spot.Id == 0)
			{
				int val;
				if (int.TryParse(spot.Text, out val) && val > 0)
				{
					CharacterCount = val;

					ParentNode.ChildHasChanged(this);
				}
			}
		}
	}

	[ContractClassFor(typeof(BaseTextNode))]
	internal abstract class BaseTextNodeContract : BaseTextNode
	{
		public override int CharacterSize
		{
			get
			{
				Contract.Ensures(Contract.Result<int>() > 0);

				throw new NotImplementedException();
			}
		}
	}
}
