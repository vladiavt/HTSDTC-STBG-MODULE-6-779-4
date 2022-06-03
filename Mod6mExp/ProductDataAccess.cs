using DataTracking.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Machine
{
    public static class ProductsDataAccess
    {
        private static SqlConnectionStringBuilder connectionStringBuilder;

        private const string productsFileName = @".\ProductsInfo.xml";

        public static List<ProductInfo> ProductsList;

        private static DataSet dataSet;

        private static SqlDataAdapter dataAdapter;

        public static void Init(string productsServerIP, string databaseName)
        {
            connectionStringBuilder = new SqlConnectionStringBuilder();
            connectionStringBuilder.DataSource = productsServerIP;
            connectionStringBuilder.InitialCatalog = databaseName;
            connectionStringBuilder.UserID = "machine";
            connectionStringBuilder.Password = "qipe";
            connectionStringBuilder.ConnectTimeout = 5;


            ProductsList = new List<ProductInfo>();
            dataSet = new DataSet();
            dataAdapter = new SqlDataAdapter();

        }

        public static OperationResult GetProducts()
        {
            var operationResult = GetProductsFromServer();
            if (operationResult.Success)
            {
                if (SaveProductsLocally() != 0)
                {
                    operationResult.AddMessage("Неуспешно записване на процесните настройки в локалната памет");
                }
            }
            else
            {
                var result = LoadProductsLocally();
                if (result == 0)
                {
                    operationResult.Success = true;
                    operationResult.AddMessage("Процесните настройки са успешно заредени от локалната памет");
                }
                else
                {
                    operationResult.Success = false;
                    if (result == 1)
                    {
                        operationResult.AddMessage("Неуспешно зареждане на процесните настройки от локалната памет.Грешка при четене от файла с процесните настройки.");

                    }
                    else if (result == 2)
                    {
                        operationResult.AddMessage("Неуспешно зареждане на процесните настройки от локалната памет.Не е открит файла с процесните настройки.");
                    }
                }
            }
            return operationResult;
        }

        public static void SaveChanges()
        {
            //int result = 0;
            if (dataSet.HasChanges())
            {

                SaveChangesToServer();
                SaveProductsLocally();

            }
            //		return result;
        }

        private static void SaveChangesToServer()
        {
            try
            {
                SqlCommandBuilder commandBuilder = new SqlCommandBuilder(dataAdapter);
                dataAdapter.Update(dataSet);

            }
            catch (Exception)
            {
                throw;

            }
        }
        private static int SaveProductsLocally()
        {

            try
            {
                GetProductsFromDataSet();
                Stream productsFileStream = File.Create(productsFileName);

                XmlSerializer serializer = new XmlSerializer(typeof(List<ProductInfo>));
                serializer.Serialize(productsFileStream, ProductsList);
                productsFileStream.Close();
                return 0;

            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static int LoadProductsLocally()
        {
            if (File.Exists(productsFileName))
            {
                try
                {
                    Stream productsFileStream = File.OpenRead(productsFileName);
                    XmlSerializer deserializer = new XmlSerializer(typeof(List<ProductInfo>));

                    ProductsList = (List<ProductInfo>)deserializer.Deserialize(productsFileStream);
                    productsFileStream.Close();
                    return 0;
                }
                catch
                {
                    return 1;
                }
            }
            else
            {
                return 2;
            }
        }

        public static DataTracking.Models.OperationResult<DataSet> GetProductsFromServer()
        {
            SqlCommand command;
            SqlConnection connectionLocal;
            connectionLocal = new SqlConnection(connectionStringBuilder.ToString());
            OperationResult<DataSet> result = new OperationResult<DataSet>();
            dataSet = new DataSet();
            try
            {
                connectionLocal.Open();
                string selectCommand = string.Format(@"SELECT [Product]
															,[L1]
															,[L2]
															,[L3]
															,[L4]
                                                            ,[MarkFN]
                                                            ,[ProductLength]
                                                            ,[CheckNut]
                                                            ,[ProductType]
														  FROM " + Properties.Settings.Default.TableProductsName);
                command = new SqlCommand(selectCommand, connectionLocal);
                dataAdapter.SelectCommand = command;
                dataAdapter.Fill(dataSet);
                result.ResultObject = dataSet.Tables[0].DataSet;
                result.Success = true;
                return result;
            }
            catch (Exception e)
            {
                result.Success = false;
                string errorMessage = string.Format("Грешка при зареждане на процесните настройки от сървъра. Грешка: {0}", e.Message);
                result.AddMessage(errorMessage);
                return result;
            }
            finally
            {
                connectionLocal.Close();
            }

        }

        public static OperationResult<ProductInfo> GetProduct(string requiredProduct)
        {
            var result = GetProductFromServer(requiredProduct);
            if (!result.Success)
            {
                int index = ProductsList.FindIndex(p => p.Product == requiredProduct);
                if (index >= 0)
                {
                    result.Success = true;
                    result.ResultObject = ProductsList[index];
                }
                else
                {
                    result.Success = false;
                    result.AddMessage(string.Format("Не са намерени процесните настройки за изделие {0} в локалната памет", requiredProduct));
                }
            }
            return result;
        }

        public static OperationResult<ProductInfo> GetProductFromServer(string requiredProduct)
        {
            SqlCommand command;
            SqlConnection connectionLocal;
            SqlDataReader reader;
            connectionLocal = new SqlConnection(connectionStringBuilder.ToString());
            OperationResult<ProductInfo> result = new OperationResult<ProductInfo>();
            try
            {
                connectionLocal.Open();


                string str = string.Format(@"SELECT [Product]
													,[L1]
													,[L2]
													,[L3]
													,[L4]
                                                    ,[MarkFN]
                                                    ,[ProductLength]
                                                    ,[CheckNut]
                                                    ,[ProductType]
											FROM " + Properties.Settings.Default.TableProductsName + " WHERE Product=@RequiredProduct");
                command = new SqlCommand(str, connectionLocal);
                command.Parameters.AddWithValue("@RequiredProduct", requiredProduct);
                reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    ProductInfo product = new ProductInfo();

                    if (reader.Read())
                    {

                        product.Product = reader["Product"].ToString();
                        product.L1 = reader["L1"].ToString();
                        product.L2 = reader["L2"].ToString();
                        product.L3 = reader["L3"].ToString();
                        product.L4 = reader["L4"].ToString();
                        product.MarkFN = reader["MarkFN"].ToString();
                        product.ProductLength = Convert.ToDouble(reader["ProductLength"]);
                        product.CheckNut = Convert.ToBoolean(reader["CheckNut"]);
                        product.ProductType = reader["ProductType"].ToString();

                    }
                    result.ResultObject = product;
                    result.Success = true;
                }
                else
                {
                    result.Success = false;
                    result.AddMessage(string.Format("Не са намерени процесните настройки за изделие {0} на сървъра", requiredProduct));
                }
                return result;
            }
            catch (Exception e)
            {
                result.Success = false;
                string errorMessage = string.Format("Грешка при зареждане на процесните настройки за изделие {0} от сървъра. Грешка: {1}", requiredProduct, e.Message);
                result.AddMessage(errorMessage);
                return result;
            }
            finally
            {
                connectionLocal.Close();
            }
        }

        private static void GetProductsFromDataSet()
        {
            var table = dataSet.Tables[0].AsEnumerable();

            foreach (var dataRow in table)
            {
                ProductInfo product = new ProductInfo();
                product.Product = dataRow["Product"].ToString();
                product.ProductType = dataRow["ProductType"].ToString();
                product.L1 = dataRow["L1"].ToString();
                product.L2 = dataRow["L2"].ToString();
                product.L3 = dataRow["L3"].ToString();
                product.L4 = dataRow["L4"].ToString();
                product.MarkFN = dataRow["MarkFN"].ToString();
                product.ProductLength = Convert.ToDouble(dataRow["ProductLength"]);
                product.CheckNut = Convert.ToBoolean(dataRow["CheckNut"]);
                product.ProductType = (dataRow["CheckNut"].ToString());
                ProductsList.Add(product);
            }
        }
    }
}
