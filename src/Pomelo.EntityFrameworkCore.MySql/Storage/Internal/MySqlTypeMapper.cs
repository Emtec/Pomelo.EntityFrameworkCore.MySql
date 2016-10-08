// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class MySqlTypeMapper : RelationalTypeMapper
    {
	    // boolean
	    private readonly RelationalTypeMapping _bit              = new RelationalTypeMapping("bit", typeof(bool), DbType.Boolean);

	    // integers
	    private readonly RelationalTypeMapping _tinyint          = new RelationalTypeMapping("tinyint", typeof(sbyte), DbType.SByte);
	    private readonly RelationalTypeMapping _utinyint         = new RelationalTypeMapping("tinyint unsigned", typeof(byte), DbType.Byte);
	    private readonly RelationalTypeMapping _smallint         = new RelationalTypeMapping("smallint", typeof(short), DbType.Int16);
	    private readonly RelationalTypeMapping _usmallint        = new RelationalTypeMapping("smallint unsigned", typeof(ushort), DbType.UInt16);
	    private readonly RelationalTypeMapping _int              = new RelationalTypeMapping("int", typeof(int), DbType.Int32);
	    private readonly RelationalTypeMapping _uint             = new RelationalTypeMapping("int unsigned", typeof(int), DbType.UInt32);
	    private readonly RelationalTypeMapping _bigint           = new RelationalTypeMapping("bigint", typeof(long), DbType.Int64);
	    private readonly RelationalTypeMapping _ubigint          = new RelationalTypeMapping("bigint unsigned", typeof(ulong), DbType.UInt64);

	    // decimals
	    private readonly RelationalTypeMapping _decimal          = new RelationalTypeMapping("decimal(65, 30)", typeof(decimal), DbType.Decimal);
	    private readonly RelationalTypeMapping _double           = new RelationalTypeMapping("double", typeof(double), DbType.Double);
	    private readonly RelationalTypeMapping _float            = new RelationalTypeMapping("float", typeof(float));

	    // binary
	    private readonly MySqlMaxLengthMapping _char             = new MySqlMaxLengthMapping("char", typeof(char), DbType.AnsiStringFixedLength);
	    private readonly RelationalTypeMapping _varbinary        = new RelationalTypeMapping("varbinary", typeof(byte[]), DbType.Binary);
	    private readonly MySqlMaxLengthMapping _varbinary767     = new MySqlMaxLengthMapping("varbinary(767)", typeof(byte[]), DbType.Binary);
	    private readonly RelationalTypeMapping _varbinarymax     = new RelationalTypeMapping("longblob", typeof(byte[]), DbType.Binary);

	    // string
	    private readonly MySqlMaxLengthMapping _varchar          = new MySqlMaxLengthMapping("varchar", typeof(string), DbType.AnsiString, false, 255);
        private readonly RelationalTypeMapping _tinytext         = new RelationalTypeMapping("tinytext", typeof(string));
        private readonly RelationalTypeMapping _text             = new RelationalTypeMapping("text", typeof(string));
        private readonly RelationalTypeMapping _mediumtext       = new RelationalTypeMapping("mediumtext", typeof(string));
        private readonly RelationalTypeMapping _longtext         = new RelationalTypeMapping("longtext", typeof(string));

	    // DateTime
	    private readonly RelationalTypeMapping _datetime         = new RelationalTypeMapping("datetime", typeof(DateTime), DbType.DateTime);
	    private readonly RelationalTypeMapping _datetimeoffset   = new RelationalTypeMapping("bigint", typeof(DateTimeOffset), DbType.Int64);
	    private readonly RelationalTypeMapping _time             = new RelationalTypeMapping("time(6)", typeof(TimeSpan), DbType.Time);

	    // json
	    private readonly RelationalTypeMapping _json             = new RelationalTypeMapping("json", typeof(JsonObject<>), DbType.String);

	    // row version
	    private readonly RelationalTypeMapping _rowversion       = new RelationalTypeMapping("TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP", typeof(byte[]), DbType.Binary);

	    // guid
	    private readonly RelationalTypeMapping _uniqueidentifier = new RelationalTypeMapping("char(36)", typeof(Guid));

        readonly Dictionary<string, RelationalTypeMapping> _simpleNameMappings;
        readonly Dictionary<Type, RelationalTypeMapping> _simpleMappings;

        public MySqlTypeMapper()
        {
            _simpleNameMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                {
                    // boolean
                    { "bit", _bit },

                    // integers
                    { "tinyint", _tinyint },
                    { "smallint", _smallint },
                    { "int", _int },
                    { "bigint", _bigint },

                    // decimals
                    { "decimal", _decimal },
                    { "double", _double },
                    { "float", _float },

                    // binary
                    { "binary", _varbinary },
                    { "char", _char },
                    { "varbinary", _varbinary },
                    { "varchar", _varchar },

                    // string
                    { "tinytext", _tinytext },
                    { "text", _text },
                    { "mediumtext", _mediumtext },
                    { "longtext", _longtext },

                    // DateTime
                    { "datetime", _datetime },
                    { "date", _datetime },
                    { "time", _time },

                    // Json
                    { "json", _json },

                    // Guid
                    { "uniqueidentifier", _uniqueidentifier },
                };

            _simpleMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
	                // boolean
	                { typeof(bool), _bit },

	                // integers
	                { typeof(short), _smallint },
	                { typeof(ushort), _usmallint },
	                { typeof(int), _int },
	                { typeof(uint), _uint },
	                { typeof(long), _bigint },
	                { typeof(ulong), _ubigint },

	                // decimals
	                { typeof(decimal), _decimal },
	                { typeof(float), _float },
	                { typeof(double), _double },

	                // byte / char
	                { typeof(sbyte), _tinyint },
	                { typeof(byte), _utinyint },
	                { typeof(char), _utinyint },

                    // string
                    { typeof(string), _varchar },

	                // DateTime
	                { typeof(DateTime), _datetime },
	                { typeof(DateTimeOffset), _datetimeoffset },
	                { typeof(TimeSpan), _time },

	                // json
	                { typeof(JsonObject<>), _json },

	                // guid
	                { typeof(Guid), _uniqueidentifier }
                };

            ByteArrayMapper
                = new ByteArrayRelationalTypeMapper(
                    8000,
                    _varbinarymax,
                    _varbinary767,
                    _varbinary767,
                    _rowversion, size => new MySqlMaxLengthMapping(
                        "varbinary(" + size + ")",
                        typeof(byte[]),
                        DbType.Binary,
                        unicode: false,
                        size: size,
                        hasNonDefaultUnicode: false,
                        hasNonDefaultSize: true));
        }

        protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings()
        {
            return _simpleNameMappings;
        }

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
        {
            return _simpleMappings;
        }

        public override IByteArrayRelationalTypeMapper ByteArrayMapper { get; }

        protected override string GetColumnType(IProperty property) => property.MySql().ColumnType;

        public override RelationalTypeMapping FindMapping(Type clrType)
        {
            Check.NotNull(clrType, nameof(clrType));
            
            if (clrType.Name == typeof(JsonObject<>).Name)
                return _json;

            return (clrType == typeof(byte[]) ? _varbinarymax : base.FindMapping(clrType));
        }

        protected override RelationalTypeMapping FindCustomMapping([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var clrType = property.ClrType.UnwrapEnumType();

            if (clrType == typeof(string))
                return StringMapping(property);

            if (clrType == typeof(byte[]))
                return ByteArrayMapper.FindMapping(false, (property.IsKey() || property.IsIndex()), property.GetMaxLength());

            return base.FindCustomMapping(property);
        }

        private RelationalTypeMapping StringMapping([NotNull] IProperty property)
        {
            var max = property.GetMaxLength() ?? 255;

            if (max > 0)
            {
                if (max <= 65535)
                    return new RelationalTypeMapping("varchar(" + max + ")", typeof(string), DbType.AnsiString, false, max);

                if (max <= 16777215)
                    return _mediumtext;

                return _longtext;
            }
            
            throw new ArgumentException($"Invalid max length for string type (must be greater than 0): {max}");
        }

    }
}
