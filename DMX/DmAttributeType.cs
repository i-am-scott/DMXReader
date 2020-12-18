namespace DMXReader.DMX
{
    public enum DmAttributeType_t
    {
        AT_UNKNOWN = 0,
        AT_FIRST_VALUE_TYPE,
        AT_ELEMENT = AT_FIRST_VALUE_TYPE,
        AT_INT,
        AT_FLOAT,
        AT_BOOL,
        AT_STRING,
        AT_VOID,
        AT_TIME,
        AT_COLOR, //rgba
        AT_VECTOR2,
        AT_VECTOR3,
        AT_VECTOR4,
        AT_QANGLE,
        AT_QUATERNION,
        AT_VMATRIX,
        AT_FIRST_ARRAY_TYPE,
        AT_ELEMENT_ARRAY = AT_FIRST_ARRAY_TYPE,
        AT_INT_ARRAY,
        AT_FLOAT_ARRAY,
        AT_BOOL_ARRAY,
        AT_STRING_ARRAY,
        AT_VOID_ARRAY,
        AT_TIME_ARRAY,
        AT_COLOR_ARRAY,
        AT_VECTOR2_ARRAY,
        AT_VECTOR3_ARRAY,
        AT_VECTOR4_ARRAY,
        AT_QANGLE_ARRAY,
        AT_QUATERNION_ARRAY,
        AT_VMATRIX_ARRAY,
        AT_TYPE_COUNT,
        AT_TYPE_INVALID,
    };
}