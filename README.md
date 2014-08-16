DBConnection
=============

C# db connection lib

Usage
=====
	var _db=new DBConnection(dbConstr);

	string sql="select * from dbo.test";
	DataTable dt=_db.GetSingleData(sql);
	Test t=_db.GetSingleData<Test>(sql); // auto fill field and property value with corresponding field in data table
	Test t2=_db.GetSingleData<Test>(sql,dr => new Test(dr["type"]));
	// Test is customized class
	
	string spname="sp_test";
	Dictionary<string,string> param=new Dictionary<string,string>(){
		{"id","1"},{"type","test"}
	}
	DataTable dt=_db.ExecSP(spname, param);
	Test t3=_db.ExecSP<Test>(spname, param);
	Test t4=_db.ExecSP<Test>(spname, param,dr => new Test(){ value=dr["type"].ToString()});
	
	private class Test {
		public Test(){}
		public Test(string v){
			value=v;
		}
		public valus;
		public value2{get;set;}
	}
Change Log
==========

