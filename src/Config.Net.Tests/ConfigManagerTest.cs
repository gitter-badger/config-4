﻿using System;
using Config.Net.Stores;
using NUnit.Framework;

namespace Config.Net.Tests
{
   [TestFixture]
   class ConfigManagerTest
   {
      // ReSharper disable InconsistentNaming
      enum Grid
      {
         IT,
         AC,
         UK,
         US,
         ZA
      }
      // ReSharper restore InconsistentNaming

      private static readonly Setting<string> UnitTestName = new Setting<string>("UnitTestName", "not set");
      private static readonly Setting<int> NumberOfMinutes = new Setting<int>("NumberOfMinutes", 10);
      private static readonly Setting<string[]> Regions = new Setting<string[]>("Regions", new [] {"Japan", "Denmark", "Australia"});
      private static readonly Setting<bool> LogXml = new Setting<bool>("log-xml", true);
      private static readonly Setting<int?> NumberOfMinutesMaybe = new Setting<int?>("NumberOfMinutesMaybe", null); 
      private static readonly Setting<TimeSpan> PingInterval = new Setting<TimeSpan>("ping-interval", TimeSpan.FromMinutes(1)); 
      private static readonly Setting<JiraTime> IssueEstimate = new Setting<JiraTime>("estimate", JiraTime.FromHumanReadableString("1h2m")); 
      private static readonly Setting<Grid> ActiveGrid = new Setting<Grid>("ActiveGrid", Grid.ZA);
      private static readonly Setting<Grid?> ActiveGridMaybe = new Setting<Grid?>("ActiveGridMaybe", null);
      private static readonly Setting<string> WithAlternativeKeyNames = new Setting<string>("Key1", null)
      {
         AlsoKnownAs = new[] {"NewKey1", "OldKey1"}
      };
 
      private TestStore _store;

      [SetUp]
      public void SetUp()
      {
         Cfg.Configuration.RemoveAllStores();
         _store = new TestStore();
         Cfg.Configuration.AddStore(_store);
         Cfg.Configuration.CacheTimeout = TimeSpan.Zero;
      }

      [Test]
      public void Read_DefaultValue_Returns()
      {
         string v = Cfg.Default.Read(UnitTestName);
         Assert.AreEqual(UnitTestName.DefaultValue, v);
      }

      [Test]
      public void Read_ConfiguredValue_Returns()
      {
         _store.Map[UnitTestName.Name] = "configured value";
         Assert.AreEqual("configured value", Cfg.Default.Read(UnitTestName).Value);
      }

      [Test]
      public void Read_Integer_Reads()
      {
         _store.Map["NumberOfMinutes"] = "78";

         int minutes = Cfg.Default.Read(NumberOfMinutes);
         Assert.AreEqual(78, minutes);
      }

      [Test]
      public void Read_AlternativeKeys_Reads()
      {
         _store.Map[WithAlternativeKeyNames.AlsoKnownAs[1]] = "66";

         Assert.AreEqual("66", Cfg.Default.Read(WithAlternativeKeyNames).Value);
      }

      [Test]
      public void Read_DefaultInteger_Reads()
      {
         Assert.AreEqual(10, Cfg.Default.Read(NumberOfMinutes).Value);
      }

      [Test]
      public void Read_StringArray_Reads()
      {
         _store.Map["Regions"] = "IT, UK, US";

         string[] regions = Cfg.Default.Read(Regions);

         Assert.AreEqual(3, regions.Length);
      }

      [Test]
      public void ReadBooleanTrueFalseTest()
      {
         _store.Map["log-xml"] = "true";
         Assert.IsTrue(Cfg.Default.Read(LogXml));

         _store.Map["log-xml"] = "false";
         Assert.IsFalse(Cfg.Default.Read(LogXml));
      }

      [Test]
      public void ReadBooleanYesNoTest()
      {
         _store.Map["log-xml"] = "yes";
         Assert.IsTrue(Cfg.Read(LogXml));

         _store.Map["log-xml"] = "no";
         Assert.IsFalse(Cfg.Default.Read(LogXml));         
      }

      [Test]
      public void Read_PropertySyntax_Reads()
      {
         _store.Map["log-xml"] = "no";
         Assert.IsFalse(LogXml);
      }

      [Test]
      public void ReadBoolean10Test()
      {
         _store.Map["log-xml"] = "1";
         Assert.IsTrue(Cfg.Default.Read(LogXml));

         _store.Map["log-xml"] = "0";
         Assert.IsFalse(Cfg.Default.Read(LogXml));
      }

      [Test]
      public void TimeSpanParserTest()
      {
         _store.Map["ping-interval"] = "01:02:03";
         TimeSpan v = Cfg.Default.Read(PingInterval);
         Assert.AreEqual(1, v.Hours);
         Assert.AreEqual(2, v.Minutes);
         Assert.AreEqual(3, v.Seconds);
      }

      [Test]
      public void JiraTimeParserTest()
      {
         _store.Map["estimate"] = "1d4h";
         JiraTime time = Cfg.Default.Read(IssueEstimate);
         Assert.AreEqual(1, time.TimeSpan.Days);
         Assert.AreEqual(4, time.TimeSpan.Hours);
         Assert.AreEqual(0, time.TimeSpan.Minutes);
         Assert.AreEqual(0, time.TimeSpan.Seconds);
         Assert.AreEqual(0, time.TimeSpan.Milliseconds);
      }

      [Test]
      public void ReadEnum_NotInConfig_DefaultValue()
      {
         Grid grid = Cfg.Read(ActiveGrid);
         Assert.AreEqual(Grid.ZA, grid);
      }

      [Test]
      public void ReadEnum_InConfig_ConfigValue()
      {
         _store.Map["ActiveGrid"] = "UK";
         Grid grid = Cfg.Read(ActiveGrid);
         Assert.AreEqual(Grid.UK, grid);
      }

      [Test]
      public void ReadEnum_InConfigInWrongCase_ConfigValue()
      {
         _store.Map["ActiveGrid"] = "uK";
         Grid grid = Cfg.Read(ActiveGrid);
         Assert.AreEqual(Grid.UK, grid);
      }

      [Test]
      public void ReadEnum_OutOfRange_DefaultValue()
      {
         _store.Map["ActiveGrid"] = "dfdsfdsfdsf";
         Grid grid = Cfg.Read(ActiveGrid);
         Assert.AreEqual(Grid.ZA, grid);
      }

      [Test]
      public void ReadEnum_Null_DefaultValue()
      {
         _store.Map["ActiveGrid"] = null;
         Grid grid = Cfg.Read(ActiveGrid);
         Assert.AreEqual(Grid.ZA, grid);
      }

      [Test]
      public void ReadNullableEnum_Null_Null()
      {
         Grid? grid = Cfg.Read(ActiveGridMaybe);
         Assert.IsNull(grid);
      }

      [Test]
      public void ReadNullableEnum_NotNull_CorrectValue()
      {
         _store.Map[ActiveGridMaybe.Name] = Grid.ZA.ToString();
         Assert.AreEqual(Grid.ZA, Cfg.Read(ActiveGridMaybe).Value);
      }

      [Test]
      public void ReadNullableEnum_OutOfRange_Null()
      {
         _store.Map[ActiveGridMaybe.Name] = "Out Of Range";
         Assert.IsNull(Cfg.Read(ActiveGridMaybe).Value);
      }

      [Test]
      public void ReadNullableInt_Null_Null()
      {
         int? value = Cfg.Read(NumberOfMinutesMaybe);
         Assert.IsNull(value);
      }

      [Test]
      public void ReadNullableInt_NotNull_CorrectValue()
      {
         _store.Map[NumberOfMinutesMaybe.Name] = "9";
         Assert.AreEqual(9, Cfg.Read(NumberOfMinutesMaybe).Value);
      }

      [Test]
      public void ReadProperty_TwoInsances_BothUpdateValue()
      {
         _store.Map["NumberOfMinutes"] = "78";
         Property<int> minutes1 = Cfg.Read(NumberOfMinutes);
         Assert.AreEqual(78, (int)minutes1);

         //now change property value and check it's updated in first and second instance
         _store.Map["NumberOfMinutes"] = "79";
         Property<int> minutes2 = Cfg.Read(NumberOfMinutes);
         Assert.AreEqual(79, (int)minutes2);
         Assert.AreEqual(79, (int)minutes1);
      }

      [Test]
      public void ReadProperty_ValueChanges_EventThrown()
      {
         _store.Map["NumberOfMinutes"] = "78";
         Property<int> minutes = Cfg.Read(NumberOfMinutes);
         Assert.AreEqual(78, (int)minutes);

         bool thrown = false;
         minutes.ValueChanged += (v) => thrown = true;
         _store.Map["NumberOfMinutes"] = "80";
         minutes = Cfg.Read(NumberOfMinutes);

         Assert.AreEqual(80, (int)minutes);
         Assert.IsTrue(thrown);
      }

      [Test]
      public void ReadProperty_ValueNotChanged_EventNotThrown()
      {
         _store.Map["NumberOfMinutes"] = "78";
         Property<int> minutes = Cfg.Read(NumberOfMinutes);
         Assert.AreEqual(78, (int)minutes);

         bool thrown = false;
         minutes.ValueChanged += (v) => thrown = true;
         minutes = Cfg.Read(NumberOfMinutes);

         Assert.AreEqual(78, (int)minutes);
         Assert.IsFalse(thrown);
      }

      /// <summary>
      /// Previously this operation would fail because ConfigManager would compare the cached value to
      /// a newly read one and fail because string arrays don't implement IComparable
      /// </summary>
      [Test]
      public void ReadStringArray_Twice_DoesntFail()
      {
         _store.Map["Regions"] = "IT, UK, US";

         Cfg.Default.Read(Regions);
         Cfg.Default.Read(Regions);
      }

      [Test]
      public void PropertyIntIsDefault_WhenDefaultValue_ReturnsTrue()
      {
         _store.Map["NumberOfMinutes"] = "10";
         
         var numberofMinutes = Cfg.Read(NumberOfMinutes);
         
         Assert.IsTrue(numberofMinutes.IsDefaultValue);
      }

      [Test]
      public void PropertyIntIsDefault_WhenDefaultValue_ReturnsFalse()
      {
         _store.Map["NumberOfMinutes"] = "15";

         var numberofMinutes = Cfg.Read(NumberOfMinutes);

         Assert.IsFalse(numberofMinutes.IsDefaultValue);
      }

      [Test]
      public void PropertyGridIsDefault_WhenDefaultValue_ReturnsTrue()
      {
         _store.Map["ActiveGrid"] = Grid.ZA.ToString();

         Property<Grid> activeGrid = Cfg.Read(ActiveGrid);

         Assert.IsTrue(activeGrid.IsDefaultValue);
      }

      [Test]
      public void PropertyGridIsDefault_WhenDefaultValue_ReturnsFalse()
      {
         _store.Map["ActiveGrid"] = Grid.US.ToString();

         Property<Grid> activeGrid = Cfg.Read(ActiveGrid);

         Assert.IsFalse(activeGrid.IsDefaultValue);
      }

      [Test]
      public void PropertyStringIsDefault_WhenDefaultValue_ReturnsTrue()
      {
         _store.Map["UnitTestName"] = "not set";
         
         Property<string> unitTestName = Cfg.Read(UnitTestName);

         Assert.IsTrue(unitTestName.IsDefaultValue);
      }

      [Test]
      public void PropertyStringIsDefault_WhenDefaultValue_ReturnsFalse()
      {
         _store.Map["UnitTestName"] = "UnitTestName";

         Property<string> unitTestName = Cfg.Read(UnitTestName);

         Assert.IsFalse(unitTestName.IsDefaultValue);
      }

      [Test]
      public void PropertyStringArrayIsDefault_WhenDefaultValue_ReturnsTrue()
      {
         _store.Map["Regions"] = null;

         Property<string[]> regions = Cfg.Read(Regions);

         Assert.IsTrue(regions.IsDefaultValue);
      }

      [Test]
      public void PropertyStringArrayIsDefault_WhenDefaultValue_ReturnsFalse()
      {
         _store.Map["Regions"] = "UK, JP, ZA";

         Property<string[]> regions = Cfg.Read(Regions);

         Assert.IsFalse(regions.IsDefaultValue);
      }

      [Test]
      public void PropertyBoolIsDefault_WhenDefaultValue_ReturnsTrue()
      {
         _store.Map["log-xml"] = "yes";

         Property<bool> logXml = Cfg.Read(LogXml);

         Assert.IsTrue(logXml.IsDefaultValue);
      }

      [Test]
      public void PropertyBoolIsDefault_WhenDefaultValue_ReturnsFalse()
      {
         _store.Map["log-xml"] = "no";

         Property<bool> logXml = Cfg.Read(LogXml);

         Assert.IsFalse(logXml.IsDefaultValue);
      }

      [Test]
      public void PropertyTimeSpanIsDefault_WhenDefaultValue_ReturnsTrue()
      {
         _store.Map["ping-interval"] = TimeSpan.FromMinutes(1).ToString();

         Property<TimeSpan> pingInterval = Cfg.Read(PingInterval);

         Assert.IsTrue(pingInterval.IsDefaultValue);
      }

      [Test]
      public void PropertyTimeSpanIsDefault_WhenDefaultValue_ReturnsFalse()
      {
         _store.Map["ping-interval"] = TimeSpan.FromHours(5).ToString();

         Property<TimeSpan> pingInterval = Cfg.Read(PingInterval);

         Assert.IsFalse(pingInterval.IsDefaultValue);
      }

      [Test]
      public void Write_WhenTypeNotSupported_ThrowsException()
      {
         Setting<Guid> someGuidSetting = new Setting<Guid>("MseTestGUID", Guid.NewGuid());

         Assert.Throws(typeof (ArgumentException), () => Cfg.Default.Write(someGuidSetting, Guid.NewGuid()));
      }

      [Test]
      public void Write_WhenKeyNull_ThrowsException()
      {
         Assert.Throws(typeof(ArgumentNullException), () => Cfg.Default.Write(null, "KeyIsNull"));
      }

      [Test]
      public void Write_Nullable_WhenTypeNotSupported_ThrowsException()
      {
         Setting<Guid?> someNullableGuidSetting = new Setting<Guid?>("MseTestNullableGUID", null);

         Assert.Throws(typeof(ArgumentException), () => Cfg.Default.Write(someNullableGuidSetting, Guid.NewGuid()));
      }

      [Test]
      public void WriteStringTest()
      {
         const string writeValue = "SomeValue";
         Cfg.Default.Write(UnitTestName, writeValue);
         
         Assert.AreEqual(writeValue, Cfg.Read(UnitTestName).Value);
      }

      [Test]
      public void WriteStringArrayTest()
      {
         string[] writeValue = {"Japan", "Denmark", "Australia"};
         Cfg.Default.Write(Regions, writeValue);
         
         Assert.AreEqual(writeValue, Cfg.Read(Regions).Value);
      }

      [Test]
      public void WriteIntTest()
      {
         const int writeValue = 23;
         Cfg.Default.Write(NumberOfMinutes, writeValue);

         Assert.AreEqual(writeValue, Cfg.Read(NumberOfMinutes).Value);
      }

      [Test]
      public void WriteBoolTest()
      {
         const bool writeValue = false;
         Cfg.Default.Write(LogXml, writeValue);

         Assert.AreEqual(writeValue, Cfg.Read(LogXml).Value);
      }

      [Test]
      public void WriteTimeSpanTest()
      {
         TimeSpan writeValue = TimeSpan.FromDays(23);
         Cfg.Default.Write(PingInterval, writeValue);

         Assert.AreEqual(writeValue, Cfg.Read(PingInterval).Value);
      }

      [Test]
      public void WriteJiraTimeTest()
      {
         var writeValue = new JiraTime(TimeSpan.FromDays(17));
         Cfg.Default.Write(IssueEstimate, writeValue);

         Assert.AreEqual(writeValue.ToString(), Cfg.Read(IssueEstimate).Value.ToString());
      }

      [Test]
      public void WriteEnumTest()
      {
         const Grid writeValue = Grid.UK;
         Cfg.Default.Write(ActiveGrid, writeValue);

         Assert.AreEqual(writeValue, Cfg.Read(ActiveGrid).Value);
      }

      [Test]
      public void WriteNullableIntTest()
      {
         Cfg.Default.Write(NumberOfMinutesMaybe, null);
         
         Assert.AreEqual(null, Cfg.Read(NumberOfMinutesMaybe).Value);
         _store.Map["NumberOfMinutesMaybe"] = "34";
         int? newWriteValue = 34;

         Cfg.Default.Write(NumberOfMinutesMaybe, newWriteValue);

         Assert.AreEqual(newWriteValue, Cfg.Read(NumberOfMinutesMaybe).Value);
      }

      [Test]
      public void WriteNullableEnumTest()
      {
         Cfg.Default.Write(ActiveGridMaybe, null);

         Assert.AreEqual(null, Cfg.Read(ActiveGridMaybe).Value);
         
         Grid? newWriteValue = Grid.AC;

         Cfg.Default.Write(ActiveGridMaybe, newWriteValue);

         Assert.AreEqual(newWriteValue, Cfg.Read(ActiveGridMaybe).Value);
      }

      [Test]
      public void WriteNullableTimeSpanTest()
      {
         Setting<TimeSpan?> someGuidSetting = new Setting<TimeSpan?>("MseTestNullableTimeSpan", new TimeSpan(3, 5, 58));

         Cfg.Default.Write(someGuidSetting, null);

         Assert.AreEqual(null, Cfg.Read(someGuidSetting).Value);
      }

      [Test]
      public void Write_SetSomeArrayValueAndThenSetToDefault_ReadsNull()
      {
         //Act
         string[] newValue = {"UK", "US"};

         //Arrange
         Cfg.Default.Write(Regions, newValue); //This is the first step so we write a non-default value
         Cfg.Default.Write(Regions, Regions.DefaultValue);

         //Assert
         Assert.IsNull(_store.Read("Regions"));
      }

      [Test]
      public void Write_SetSomeIntValueAndThenSetToDefault_ReadsNull()
      {
         int newValue = 12;

         Cfg.Default.Write(NumberOfMinutes, newValue); //This is the first step so we write a non-default value
         Cfg.Default.Write(NumberOfMinutes, NumberOfMinutes.DefaultValue);

         Assert.IsNull(_store.Read("NumberOfMinutes"));
      }

      [Test]
      public void Write_SetSomeEnumValueAndThenSetToDefault_ReadsNull()
      {
         Grid newValue = Grid.IT;

         Cfg.Default.Write(ActiveGrid, newValue); //This is the first step so we write a non-default value
         Cfg.Default.Write(ActiveGrid, ActiveGrid.DefaultValue);

         Assert.IsNull(_store.Read("ActiveGrid"));
      }

      [Test]
      public void Write_SetSomeTimeSpanValueAndThenSetToDefault_ReadsNull()
      {
         TimeSpan newValue = new TimeSpan(1, 1, 1);

         Cfg.Default.Write(PingInterval, newValue); //This is the first step so we write a non-default value
         Cfg.Default.Write(PingInterval, PingInterval.DefaultValue);

         Assert.IsNull(_store.Read("ping-interval"));
      }

      [Test]
      public void RemoveStores_AddOneStoreReadsKey_RemoveStoreKeyWontRead()
      {
         var store = new InMemoryConfigStore();
         string keyName = Guid.NewGuid().ToString();
         var setting = new Setting<string>(keyName, null);

         Assert.IsNull(Cfg.Read(setting).Value);

         store.Write(keyName, "12345");
         Cfg.Configuration.AddStore(store);
         Assert.AreEqual("12345", Cfg.Read(setting).Value);

         Cfg.Configuration.RemoveStore(store.Name);
         Assert.IsNull(Cfg.Read(setting).Value);
      }
   }
}
