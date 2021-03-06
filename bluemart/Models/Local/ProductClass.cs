﻿using System;
using SQLite;
using bluemart.Common.Utilities;
using System.Collections.Generic;

namespace bluemart.Models.Local
{
	[Table("Products")]
	public class ProductClass
	{
		[PrimaryKey]
		public string objectId { get; set; }
		public string CategoryId { get; set; }
		public string ImageID { get; set; }
		public string ImageName { get; set; }
		public string Name { get; set; }
		public decimal Price { get; set; }
		public string Quantity { get; set; }
		public string ParentCategory{ get; set; }
		public bool IsTopSelling{ get; set; }
		public int Priority{ get; set; }
		public string Stores { get; set; }
		public bool IsInStock { get; set; }

		private bool TableExists<T> (SQLiteConnection connection,string tableName)
		{    
			const string cmdText = "SELECT name FROM sqlite_master WHERE type='table' AND name=?";
			var cmd = connection.CreateCommand (cmdText, tableName);
			return cmd.ExecuteScalar<string> () != null;
		}

		public void AddProduct( List<ProductClass> productList  )
		{
			var db = new SQLiteConnection (DBConstants.DB_PATH);

			if (!TableExists<ProductClass> (db,"Products"))
			{
				db.CreateTable<ProductClass>();
			}
			db.InsertAll (productList,"OR REPLACE", true);
			db.Close ();
		}

		public List<ProductClass> GetProducts()
		{
			List<ProductClass> productList = new List<ProductClass> ();

			var db = new SQLiteConnection (DBConstants.DB_PATH);
		

			if (!TableExists<ProductClass> (db,"Products")) {
				db.Close ();
				return productList;		
				//db.CreateTable<ProductClass>();

			}
			productList = db.Query<ProductClass> ("SELECT * FROM Products ORDER BY Priority, Name");

			/*var query = from Products in  db.Table<ProductClass> ()
				orderby Products.Priority,Products.Name
				select Products;

			foreach (var product in query) {
				productList.Add (product);
			}
*/
			db.Close ();

			return productList;
			//return query;
		}
	}
}

