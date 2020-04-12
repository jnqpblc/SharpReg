using System;
using System.IO;
using System.Net;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Linq;

namespace SharpSvc
{
	class SharpSvc
	{
		// https://docs.microsoft.com/en-us/dotnet/api/microsoft.win32.registrykey?view=netframework-4.6.1
		// https://stackoverflow.com/questions/28739477/accessing-a-remote-registry-with-local-credentials-of-the-remote-machine-on-th
		// https://social.msdn.microsoft.com/Forums/vstudio/en-US/e7b672e6-bd55-41aa-b154-8a51e7b9159f/how-to-connect-and-access-a-drive-in-remote-server-from-my-local-machine-using-credentials-in-cnet?forum=csharpgeneral
		static void Main(string[] args)
		{
			if (args == null || args.Length < 1)
			{
				printUsage();
			}

			if ((args[0].ToUpper() == "--QUERY") && (args.Length >= 4))
			{
				string Computer = args[1];
				string KeyName = args[2];
				string ValueName = args[3];
				if (args.Length == 5)
				{
					string SearchTeam = args[4];
					Query(Computer, KeyName, ValueName, SearchTeam);
				}
				else
				{
					Query(Computer, KeyName, ValueName, null);
				}
			}
			else if ((args[0].ToUpper() == "--ADD") && (args.Length == 6))
			{
				string Computer = args[1];
				string KeyName = args[2];
				string DataType = args[3];
				string ValueName = args[4];
				string ValueData = args[5];
				Add(Computer, KeyName, DataType, ValueName, ValueData);
			}
			else if ((args[0].ToUpper() == "--DELETE") && (args.Length == 4))
			{
				string Computer = args[1];
				string KeyName = args[2];
				string ValueName = args[3];
				Delete(Computer, KeyName, ValueName);
			}
			else if ((args[0].ToUpper() == "--PERSIST") && (args.Length == 3))
			{
				string Computer = args[1];
				string ValueName = args[2];
				Persist(Computer, ValueName);
			}
			else
			{
				printUsage();
			}
		}

		static void printUsage()
		{
			Console.WriteLine("\n[-] Usage: \n\t--Query <Computer|local|hostname|ip> <KeyName|SOFTWARE\\Microsoft\\Policies> <ValueName|count|all|recurse|grep|ScriptBlockLogging> <SearchTeam|Grep() Only|E.g. \"Google\">\n" +
				"\n\t--Add <Computer|local|hostname|ip> <KeyName|SOFTWARE\\Microsoft\\Policies> <DataType|SZ|EXPAND_SZ|DWORD|QWORD|BINARY> <ValueName|YourValueName> <ValueData|YourValueData>\n" +
				"\n\t--Delete <Computer|local|hostname|ip> <KeyName|SOFTWARE\\Microsoft\\Policies> <ValueName|all|ScriptBlockLogging>\n" +
				"\n\t--Persist <Computer|local|hostname|ip> <ValueName|netsvcs>\n");
			System.Environment.Exit(1);
		}

		static void Query(string Computer, string KeyName, string ValueName, string SearchTeam)
		{
			try
			{
				RegistryKey hive;
				if (Computer.ToUpper() != "LOCAL")
				{
					hive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, Computer, RegistryView.Default);
				}
				else
				{
					hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
				}
				var key = hive.OpenSubKey(KeyName);
				if (ValueName.ToUpper() == "COUNT")
				{
					try
					{
						Console.WriteLine("\nThere are {0} subkeys under {1}.", key.SubKeyCount.ToString(), key.Name);
						hive.Close();
						return;
					}
					catch { } // Used to ignore exceptions
				}
				else if (ValueName.ToUpper() == "PERMS")
				{
					try
					{
						RegistrySecurity registrySecurity = key.GetAccessControl();
						Console.WriteLine("\n{0}\n", key.Name);
						Console.WriteLine("[*] None:\n{0}\n", registrySecurity.GetSecurityDescriptorSddlForm(AccessControlSections.None));
						Console.WriteLine("[*] Audit:\n{0}\n", registrySecurity.GetSecurityDescriptorSddlForm(AccessControlSections.Audit));
						Console.WriteLine("[*] Access:\n{0}\n", registrySecurity.GetSecurityDescriptorSddlForm(AccessControlSections.Access));
						Console.WriteLine("[*] Group:\n{0}\n", registrySecurity.GetSecurityDescriptorSddlForm(AccessControlSections.Group));
						var rules = registrySecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
						foreach (var rule in rules.Cast<AuthorizationRule>())
						{
							Console.WriteLine("{0}", rule.IdentityReference.Value);
						}
						hive.Close();
						return;
					}
					catch { } // Used to ignore exceptions
				}
				else if (ValueName.ToUpper() == "ALL")
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
					hive.Close();
					return;
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
					hive.Close();
					return;
				}
				else if (ValueName.ToUpper() == "GREP")
				{
					Console.WriteLine();
					foreach (string oVal in key.GetValueNames())
					{
						if (oVal.Contains(SearchTeam))
						{
							try
							{
								Console.WriteLine("    {0}    REG_{1}    {2}", oVal, key.GetValueKind(oVal).ToString().ToUpper(), key.GetValue(oVal).ToString());
							}
							catch { } // Used to ignore exceptions
						}
					}
					Console.WriteLine();
					foreach (string oSubKey in key.GetSubKeyNames())
					{
						if (oSubKey.Contains(SearchTeam))
						{
							Console.WriteLine("{0}\\{1}", KeyName, oSubKey);
						}
						try
						{
							var skey = hive.OpenSubKey(KeyName + "\\" + oSubKey);
							foreach (string osVal in skey.GetValueNames())
							{
								try
								{
									if (osVal.Contains(SearchTeam) || skey.GetValue(osVal).ToString().Contains(SearchTeam))
									{
										Console.WriteLine("\n{0}\\{1}", KeyName, oSubKey);
										Console.WriteLine("\n    {0}    REG_{1}    {2}", osVal, skey.GetValueKind(osVal).ToString().ToUpper(), skey.GetValue(osVal).ToString());
									}
								}
								catch { } // Used to ignore exceptions
							}
						}
						catch { } // Used to ignore exceptions
					}
					Console.WriteLine();
					hive.Close();
					return;
				}
				else
				{
					if (key.GetValueKind(ValueName).ToString().ToUpper() == "BINARY")
					{
						byte[] BinData = (byte[])key.GetValue(ValueName);
						string BinString = BitConverter.ToString(BinData).Replace("-", ""); ;
						Console.WriteLine("\n    {0}    REG_{1}    {2}", ValueName, key.GetValueKind(ValueName).ToString().ToUpper(), BinString.ToString());
					}
					else if (key.GetValueKind(ValueName).ToString().ToUpper() == "MULTISTRING")
					{
						Console.WriteLine();
						string[] tArray = (string[])key.GetValue(ValueName);
						for (int i = 0; i < tArray.Length; i++)
						{
							Console.WriteLine("    {0}    REG_{1}    {2}", ValueName, key.GetValueKind(ValueName).ToString().ToUpper(), tArray[i]);
						}
					}
					else
					{
						Console.WriteLine("\n    {0}    REG_{1}    {2}", ValueName, key.GetValueKind(ValueName).ToString().ToUpper(), key.GetValue(ValueName).ToString());
					}
					hive.Close();
					return;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("\n [!] {0}: {1}", e.GetType().Name, e.Message);
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
					hive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, Computer, RegistryView.Default);
				}
				else
				{
					hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
				}
				RegistryKey NewKey = hive.CreateSubKey(KeyName);
				if (DataType.ToUpper() == "SZ")
				{
					NewKey.SetValue(ValueName, ValueData, RegistryValueKind.String);
					Console.WriteLine("\nThe add opetation of {0} was successful.", KeyName);
					hive.Close();
					return;
				}
				else if (DataType.ToUpper() == "EXPAND_SZ")
				{
					NewKey.SetValue(ValueName, ValueData, RegistryValueKind.ExpandString);
					Console.WriteLine("\nThe add opetation of {0} was successful.", KeyName);
					hive.Close();
					return;
				}
				else if (DataType.ToUpper() == "MULTI_SZ")
				{
					//NewKey.SetValue(ValueName, ValueData, RegistryValueKind.MultiString);
					//Console.WriteLine("\nThe add opetation of {0} was successful.", KeyName);
					Console.WriteLine("\nMulti-String feature is not implemented yet.");
					hive.Close();
					return;
				}
				else if (DataType.ToUpper() == "DWORD")
				{
					NewKey.SetValue(ValueName, int.Parse(ValueData), RegistryValueKind.DWord);
					Console.WriteLine("\nThe add opetation of {0} was successful.", KeyName);
					hive.Close();
					return;
				}
				else if (DataType.ToUpper() == "QWORD")
				{
					NewKey.SetValue(ValueName, int.Parse(ValueData), RegistryValueKind.QWord);
					Console.WriteLine("\nThe add opetation of {0} was successful.", KeyName);
					hive.Close();
					return;
				}
				else if (DataType.ToUpper() == "BINARY")
				{
					byte[] ValueByte = System.Text.Encoding.ASCII.GetBytes(ValueData);
					NewKey.SetValue(ValueName, ValueByte, RegistryValueKind.Binary);
					Console.WriteLine("\nThe add opetation of {0} was successful.", KeyName);
					hive.Close();
					return;
				}
				else
				{
					printUsage();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("\n [!] {0}: {1}", e.GetType().Name, e.Message);
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
					hive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, Computer, RegistryView.Default);
				}
				else
				{
					hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
				}
				if (ValueName.ToUpper() == "KEY")
				{
					try
					{
						hive.DeleteSubKey(KeyName);
						Console.WriteLine("\nThe delete opetation of {0} was successful.", KeyName);
						hive.Close();
						return;
					}
					catch (Exception e)
					{
						Console.WriteLine("\n [!] {0}: {1}", e.GetType().Name, e.Message);
						hive.Close();
						return;
					}

				}
				else if (ValueName != null)
				{
					try
					{
						hive.OpenSubKey(KeyName, true).DeleteValue(ValueName);
						Console.WriteLine("\nThe delete opetation of {0} was successful.", ValueName);
						hive.Close();
						return;
					}
					catch (Exception e)
					{
						Console.WriteLine("\n [!] {0}: {1}", e.GetType().Name, e.Message);
						hive.Close();
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
				Console.WriteLine("\n [!] {0}: {1}", e.GetType().Name, e.Message);
				return;
			}
		}

		static void Persist(string Computer, string ValueName)
		{
			try
			{
				RegistryKey hive;
				if (Computer.ToUpper() != "LOCAL")
				{
					hive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, Computer, RegistryView.Default);
				}
				else
				{
					hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
				}
				if (ValueName.ToUpper() == "NETSVCS")
				{
					Console.WriteLine("\n Empty Parking Spaces Within Svchost:\n");
					var nkey = hive.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\SvcHost");
					string[] tArray = (string[])nkey.GetValue(ValueName);
					for (int i = 0; i < tArray.Length; i++)
					{
						if (hive.OpenSubKey(@"System\CurrentControlSet\Services\" + tArray[i]) == null)
						{
							Console.WriteLine(" [+] {0}", tArray[i]);
						}
					}

					Console.WriteLine("\n Unlocked Cars Owned By Svchost:\n");
					for (int i = 0; i < tArray.Length; i++)
					{
						try
						{
							var skey = hive.OpenSubKey(@"System\CurrentControlSet\Services\" + tArray[i]);
							if ((int)skey.GetValue("Start") == 3) // Indicates that the service is started only manually
							{
								Console.WriteLine(" [+] {0}", tArray[i]);
							}
						}
						catch { } // Used to ignore exceptions
					}
					hive.Close();
					return;
				}
				else
				{
					return;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("\n [!] {0}: {1}", e.GetType().Name, e.Message);
				return;
			}
		}

	}
}
