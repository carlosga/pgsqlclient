/*
 *  PgSqlClient - ADO.NET Data Provider for PostgreSQL 7.4+
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. 
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2003, 2006 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using PostgreSql.Data.PostgreSqlClient;

namespace PostgreSql.Data.Schema
{
    internal sealed class PgSchemaFactory
    {
        #region · Static Members ·

        private static readonly string ResName = "PostgreSql.Data.Schema.MetaData.xml";

        #endregion

        #region · Constructors ·

        private PgSchemaFactory()
        {
        }

        #endregion

        #region · Static Methods ·

        public static DataTable GetSchema(PgConnection connection, string collectionName, string[] restrictions)
        {
            string  filter      = String.Format("CollectionName = '{0}'", collectionName);
            Stream  xmlStream   = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResName);            	
            DataSet ds          = new DataSet();

            ds.ReadXml(xmlStream);            

            DataRow[] collection = ds.Tables[DbMetaDataCollectionNames.MetaDataCollections].Select(filter);

            if (collection.Length != 1)
            {
                throw new NotSupportedException("Unsupported collection name.");
            }

            if (restrictions != null && restrictions.Length > (int)collection[0]["NumberOfRestrictions"])
            {
                throw new InvalidOperationException("The number of specified restrictions is not valid.");
            }
            
            if (ds.Tables[DbMetaDataCollectionNames.Restrictions].Select(filter).Length != (int)collection[0]["NumberOfRestrictions"])
            {
                throw new InvalidOperationException("Incorrect restriction definition.");
            }

            switch (collection[0]["PopulationMechanism"].ToString())
            {
                case "PrepareCollection":
                    return PrepareCollection(connection, collectionName, restrictions);

                case "DataTable":
                    return ds.Tables[collection[0]["PopulationString"].ToString()].Copy();

                case "SQLCommand":
                    return SqlCommandCollection(connection, collectionName, (string)collection[0]["PopulationString"], restrictions);

                default:
                    throw new NotSupportedException("Unsupported population mechanism");
            }
        }

        #endregion

        #region · Schema Population Methods ·

        private static DataTable PrepareCollection(PgConnection connection, string collectionName, string[] restrictions)
        {
            PgSchema schema = null;

            switch (collectionName.Trim().ToLower())
            {
                case "checkconstraints":
                    schema = new PgCheckConstraints(connection);
                    break;
                
                case "columns":
                    schema = new PgColumns(connection);
                    break;

                case "indexes":
                    schema = new PgIndexes(connection);
                    break;

                case "indexcolumns":
                    schema = new PgIndexColumns(connection);
                    break;

                case "functions":
                    schema = new PgFunctions(connection);
                    break;

                case "functionparameters":
                    schema = new PgFunctionParameters(connection);
                    break;

                case "foreignkeys":
                    schema = new PgForeignKeys(connection);
                    break;

                case "foreignkeycolumns":
                    schema = new PgForeignKeyColumns(connection);
                    break;

                case "primarykeys":
                    schema = new PgPrimaryKeys(connection);
                    break;

                case "sequences":
                    schema = new PgSequences(connection);
                    break;

                case "tables":
                    schema = new PgTables(connection);
                    break;

                case "triggers":
                    schema = new PgTriggers(connection);
                    break;

                case "uniquekeys":
                    schema = new PgUniqueKeys(connection);
                    break;

                case "views":
                    schema = new PgViews(connection);
                    break;

                case "viewcolumns":
                    schema = new PgViewColumns(connection);
                    break;
            }

            return schema.GetSchema(collectionName, restrictions);
        }

        private static DataTable SqlCommandCollection(PgConnection connection, string collectionName, string sql, string[] restrictions)
        {
            if (restrictions == null)
            {
                restrictions = new string[0];
            }

            DataTable		dataTable	= null;
            PgDataAdapter	adapter		= null;
            PgCommand		command = new PgCommand(String.Format(sql, restrictions), connection);
            
            try
            {
                adapter = new PgDataAdapter(command);
                dataTable = new DataTable(collectionName);

                adapter.Fill(dataTable);
            }
            catch (PgException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PgException(ex.Message);
            }
            finally
            {
                command.Dispose();
                adapter.Dispose();
            }

            return dataTable;			
        }

        #endregion
    }
}