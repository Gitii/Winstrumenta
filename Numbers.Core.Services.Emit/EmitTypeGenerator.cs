using System.Reflection.Emit;
using System.Reflection;

namespace Numbers.Core.Services.Emit;

class EmitTypeGenerator : ITypeGenerator
{
    public (Type type, string[] fieldNames, IDictionary<
        string,
        string
    > map) BuildRowTypeFromHeaders(string[] headers)
    {
        var module = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName(Guid.NewGuid().ToString()),
            AssemblyBuilderAccess.Run
        );

        var mbuilder = module.DefineDynamicModule("MainModule");

        TypeBuilder tbuilder = mbuilder.DefineType(
            "Generated.Row",
            TypeAttributes.Public | TypeAttributes.Class,
            typeof(object),
            new Type[] { typeof(ICsvRowCells) }
        );

        var sanitizedHeaders = ITypeGenerator.SanitizeHeaders(headers);
        var privateFields = new List<FieldBuilder>();

        foreach (var identifier in sanitizedHeaders)
        {
            DefineAutoProperty(tbuilder, identifier);
            var privateField = DefineReadOnlyProperty(tbuilder, identifier + "Name");
            privateFields.Add(privateField);
        }

        DefineConstructorForProperties(tbuilder, privateFields.Zip(headers));

        //return (typeof(RowDummy), sanitizedHeaders);
        return (tbuilder.CreateType()!, sanitizedHeaders, null!);
    }

    private void DefineConstructorForProperties(
        TypeBuilder tbuilder,
        IEnumerable<(FieldBuilder field, string value)> fields
    )
    {
        Type objType = typeof(object);
        ConstructorInfo objCtor = objType.GetConstructor(Type.EmptyTypes)!;

        var constructorBuilder = tbuilder.DefineConstructor(
            MethodAttributes.Public | MethodAttributes.HideBySig,
            CallingConventions.Standard,
            Type.EmptyTypes
        );
        var ctorIl = constructorBuilder.GetILGenerator();

        ctorIl.Emit(OpCodes.Ldarg_0);
        ctorIl.Emit(OpCodes.Call, objCtor);

        foreach (var (field, value) in fields)
        {
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Ldstr, value);
            ctorIl.Emit(OpCodes.Stfld, field);
        }

        ctorIl.Emit(OpCodes.Ret);
    }

    private void DefineAutoProperty(TypeBuilder tbuilder, string propertyName)
    {
        FieldBuilder fFirst = tbuilder.DefineField(
            "_" + propertyName,
            typeof(string),
            FieldAttributes.Private
        );
        PropertyBuilder pFirst = tbuilder.DefineProperty(
            propertyName,
            PropertyAttributes.HasDefault,
            typeof(string),
            null
        );

        //Getter
        MethodBuilder mFirstGet = tbuilder.DefineMethod(
            "get_" + propertyName,
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            typeof(string),
            Type.EmptyTypes
        );
        ILGenerator getIl = mFirstGet.GetILGenerator();

        getIl.Emit(OpCodes.Ldarg_0);
        getIl.Emit(OpCodes.Ldfld, fFirst);
        getIl.Emit(OpCodes.Ret);

        //Setter
        MethodBuilder mFirstSet = tbuilder.DefineMethod(
            "set_" + propertyName,
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            null,
            new Type[] { typeof(string) }
        );

        ILGenerator setIl = mFirstSet.GetILGenerator();

        setIl.Emit(OpCodes.Ldarg_0);
        setIl.Emit(OpCodes.Ldarg_1);
        setIl.Emit(OpCodes.Stfld, fFirst);
        setIl.Emit(OpCodes.Ret);

        pFirst.SetGetMethod(mFirstGet);
        pFirst.SetSetMethod(mFirstSet);
    }

    private FieldBuilder DefineReadOnlyProperty(TypeBuilder tbuilder, string propertyName)
    {
        var fieldName = "_" + propertyName;
        FieldBuilder privateField = tbuilder.DefineField(
            fieldName,
            typeof(string),
            FieldAttributes.Private
        );
        PropertyBuilder pFirst = tbuilder.DefineProperty(
            propertyName,
            PropertyAttributes.HasDefault,
            typeof(string),
            null
        );

        //Getter
        MethodBuilder mFirstGet = tbuilder.DefineMethod(
            "get_" + propertyName,
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            typeof(string),
            Type.EmptyTypes
        );
        ILGenerator getIl = mFirstGet.GetILGenerator();

        getIl.Emit(OpCodes.Ldarg_0);
        getIl.Emit(OpCodes.Ldfld, privateField);
        getIl.Emit(OpCodes.Ret);

        pFirst.SetGetMethod(mFirstGet);

        return privateField;
    }
}
