using System;
using System.IO;
using Microsoft.Win32;

namespace SharpSvc
{
	class SharpSvc
	{
		static void Main(string[] args)
		{
			if (args == null || args.Length < 1)
			{
				printUsage();
			}

			if ((args[0] == "--Query") && (args.Length == 4))
			{
				string Computer = args[1];
				string KeyName = args[2];
				string ValueName = args[3];
				Query(Computer, KeyName, ValueName);
			}
			else if ((args[0] == "--Add") && (args.Length == 6))
			{
				string Computer = args[1];
				string KeyName = args[2];
				string DataType = args[3];
				string ValueName = args[4];
				string ValueData = args[5];
				Add(Computer, KeyName, DataType, ValueName, ValueData);
			}
			else if ((args[0] == "--Delete") && (args.Length == 4))
			{
				string Computer = args[1];
				string KeyName = args[2];
				string ValueName = args[3];
				Delete(Computer, KeyName, ValueName);
			}
			else
			{
				printUsage();
			}
		}
		static void printUsage()
		{
			Console.WriteLine("\n[-] Usage: \n\t--Query <Computer|local|hostname|ip> <KeyName|HKLM\\SOFTWARE\\Microsoft\\Policies> <ValueName|all|ScriptBlockLogging>\n" +
				"\n\t--Add <Computer|local|hostname|ip> <KeyName|HKLM\\SOFTWARE\\Microsoft\\Policies> <DataType|SZ|DWORD|BINARY> <ValueName|YourValueName> <ValueData|YourValueData>\n" +
				"\n\t--Delete <Computer|local|hostname|ip> <KeyName|HKLM\\SOFTWARE\\Microsoft\\Policies> <ValueName|all|ScriptBlockLogging>\n");
			System.Environment.Exit(1);
		}

		static void Query(string Computer, string KeyName, string ValueName)
		{
			try
			{
				RegistryKey hive;
				if (Computer.ToUpper() != "LOCAL")
				{
					hive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, Computer, RegistryView.Registry64);
				}
				else
				{
					hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
				}
				var key = hive.OpenSubKey(KeyName);
				if (ValueName.ToUpper() == "ALL")
				{
					Console.WriteLine();
					foreach (string oVal in key.GetValueNames())
					{
						Console.WriteLine("    {0}    REG_{1}    {2}", oVal, key.GetValueKind(oVal).ToString().ToUpper(), key.GetValue(oVal).ToString());
					}
					Console.WriteLine();
					foreach (string oSubKey in key.GetSubKeyNames())
					{
						Console.WriteLine("{0}\\{1}", KeyName, oSubKey);
					}
				}
				else if (ValueName.ToUpper() == "RECURSE")
				{
					Console.WriteLine();
					foreach (string oVal in key.GetValueNames())
					{
						Console.WriteLine("    {0}    REG_{1}    {2}", oVal, key.GetValueKind(oVal).ToString().ToUpper(), key.GetValue(oVal).ToString());
					}
					Console.WriteLine();
					foreach (string oSubKey in key.GetSubKeyNames())
					{
						Console.WriteLine("{0}\\{1}", KeyName, oSubKey);
						Console.WriteLine();
						var skey = hive.OpenSubKey(KeyName + "\\" + oSubKey);
						foreach (string osVal in skey.GetValueNames())
						{
							Console.WriteLine("    {0}    REG_{1}    {2}", osVal, skey.GetValueKind(osVal).ToString().ToUpper(), skey.GetValue(osVal).ToString());
						}
						Console.WriteLine();
					}
				}
				else
				{
					Console.WriteLine("\n    {0}    REG_{1}    {2}", ValueName, key.GetValueKind(ValueName).ToString().ToUpper(), key.GetValue(ValueName).ToString());
				}
				hive.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine("{0}: {1}", e.GetType().Name, e.Message);
				return;
			}
		}
		static void Add(string Computer, string KeyName, string DataType, string ValueName, string ValueData)
		{
			try
			{
				RegistryKey hive;
				if (Computer.ToUpper() != "LOCAL")
				{
					hive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, Computer, RegistryView.Registry64);
				}
				else
				{
					hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
				}
				RegistryKey NewKey = hive.CreateSubKey(KeyName);
				if (DataType.ToUpper() == "SZ")
				{
					NewKey.SetValue(ValueName, ValueData);
				}
				else if (DataType.ToUpper() == "DWORD")
				{
					NewKey.SetValue(ValueName, int.Parse(ValueData));
				}
				else if (DataType.ToUpper() == "BINARY")
				{
					byte[] ValueByte = System.Text.Encoding.ASCII.GetBytes(ValueData);
					NewKey.SetValue(ValueName, ValueByte);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("{0}: {1}", e.GetType().Name, e.Message);
				return;
			}
		}
		static void Delete(string Computer, string KeyName, string ValueName)
		{
			try
			{
				RegistryKey hive;
				if (Computer.ToUpper() != "LOCAL")
				{
					hive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, Computer, RegistryView.Registry64);
				}
				else
				{
					hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
				}
				if (ValueName.ToUpper() == "KEY")
				{
					try
					{
						hive.DeleteSubKey(KeyName);
						Console.WriteLine("\nThe delete opetation of {0} was successful.", KeyName);
					}
					catch (Exception e)
					{
						Console.WriteLine("\n{0}: {1}", e.GetType().Name, e.Message);
						return;
					}

				}
				else if (ValueName != null) 
				{
					try
					{
						hive.OpenSubKey(KeyName, true).DeleteValue(ValueName);
						Console.WriteLine("\nThe delete opetation of {0} was successful.", ValueName);
					}
					catch (Exception e)
					{
						Console.WriteLine("\n{0}: {1}", e.GetType().Name, e.Message);
						return;
					}
				}
				else
				{
					printUsage();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("{0}: {1}", e.GetType().Name, e.Message);
				return;
			}
		}
	}
}
