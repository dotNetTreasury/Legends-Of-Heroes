using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET
{
    [Config]
    public partial class SkillConfigCategory : Singleton<SkillConfigCategory>, IMerge
    {
        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        private Dictionary<long, SkillConfig> dict = new Dictionary<long, SkillConfig>();
        
        [BsonElement]
        private List<SkillConfig> list = new List<SkillConfig>();
		
        public void Merge(object o)
        {
            SkillConfigCategory s = o as SkillConfigCategory;
            
            foreach (SkillConfig config in list)
            {
                try
                {
                    this.dict.Add(GetMultiKeyMerge(config.Id, config.Level), config);
                }
                catch (Exception e)
                {
                    //Log.Console($"{GetMultiKeyMerge(config.Id, config.Level)} error:{e}");
	                Log.Error($"数据异常，策划检查多个key是否相同。{config.Id}, {config.Level}, \n{e}");
                }
                
            }
            
            this.list.Clear();
        }
		
        public SkillConfig Get(long id)
        {
            this.dict.TryGetValue(id, out SkillConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (SkillConfig)}，配置id: {id}");
            }

            return item;
        }
        
        private long GetMultiKeyMerge(int a = 0, int b = 0, int c = 0, int d = 0)
        {
	        //合并：高32位-中16位-中8位-低8位
	        return (long)a << 32 | ((long)b << 16) | ((long)c << 8) | (long)d;
        }

        public SkillConfig GetByKeys(int key1 = 0, int key2 = 0, int key3 = 0, int key4 = 0)
        {
	        long key = GetMultiKeyMerge(key1, key2, key3, key4);
            this.dict.TryGetValue(key, out SkillConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (SkillConfig)}，配置id: {key1}, {key2}, {key3}, {key4}");
            }

            return item;
	        //return Get(key);
        }
		
		
        public bool Contain(int key1 = 0, int key2 = 0, int key3 = 0, int key4 = 0)
        {
	        long key = GetMultiKeyMerge(key1, key2, key3, key4);
            return this.dict.ContainsKey(key);
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<long, SkillConfig> GetAll()
        {
            return this.dict;
        }

        public SkillConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

	public partial class SkillConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		public int Id { get; set; }
		/// <summary>技能等级</summary>
		public int Level { get; set; }
		/// <summary>技能抽象类型</summary>
		public int AbstractType { get; set; }
		/// <summary>名字</summary>
		public string Name { get; set; }
		/// <summary>描述</summary>
		public string Desc { get; set; }
		/// <summary>技能持续时间毫秒</summary>
		public int Life { get; set; }
		/// <summary>冷却时间毫秒</summary>
		public int CD { get; set; }
		/// <summary>技能参数</summary>
		public int[] Params { get; set; }

	}
}
