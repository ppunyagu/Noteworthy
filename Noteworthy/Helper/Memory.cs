using System;
using System.Collections.Generic;
using SQLite;
using Newtonsoft.Json;

namespace Noteworthy
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Memory
	{
		[PrimaryKey, AutoIncrement]
		public int RowId { get; set; }

		[JsonIgnore]
		public string Audio_path { get; set; }

		[JsonIgnore]
		public DateTime? Time { get; set; }

		[JsonIgnore]
		public int Duration { get; set; }


		[JsonIgnore]
		public string ConversationText { get; set; }

	}
}
