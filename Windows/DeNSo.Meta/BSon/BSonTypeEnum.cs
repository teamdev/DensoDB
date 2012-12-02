using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeNSo.BSon
{
  public enum BSonTypeEnum : byte
  {
    BSON_Null = 1,
    BSON_Document = 2,
    BSON_DocumentArray = 3,
    BSON_Dictionary = 4,
    BSON_Bool = 5,
    BSON_Byte = 6,
    BSON_int16 = 7,
    BSON_int32 = 8,
    BSON_int64 = 9,
    BSON_single = 10,
    BSON_double = 11,
    BSON_decimal = 12,
    BSON_GUID = 13,
    BSON_bynary = 14,
    BSON_chararray = 15,
    BSON_DateTime = 16,
    BSON_string = 17,
    BSON_ObjectId = 18,
    BSON_timestamp = 19,
    BSON_objectType = 20,
  }

  public enum BSonDocumentType : byte
  {
    BSON_Document = 2,
    BSON_DocumentArray = 3,
    BSON_Dictionary = 4,    
  }
}
