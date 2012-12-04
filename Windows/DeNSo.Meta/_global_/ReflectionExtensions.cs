using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;

public static class ReflectionExtensions
{
  private static MethodInfo _gethelper = null;
  private static MethodInfo _sethelper = null;

  private static Dictionary<Type, Dictionary<string, Delegate>> _getdelegates = new Dictionary<Type, Dictionary<string, Delegate>>();
  private static Dictionary<Type, Dictionary<string, Delegate>> _setdelegates = new Dictionary<Type, Dictionary<string, Delegate>>();

  static ReflectionExtensions()
  {
    _gethelper = typeof(ReflectionExtensions).GetMethod("InternalCreateGetDelegate", BindingFlags.Static |
                                                                                     BindingFlags.NonPublic |
                                                                                     BindingFlags.Public |
                                                                                     BindingFlags.ExactBinding);

    _sethelper = typeof(ReflectionExtensions).GetMethod("InternalCreateSetDelegate", BindingFlags.Static |
                                                                                     BindingFlags.NonPublic |
                                                                                     BindingFlags.Public |
                                                                                     BindingFlags.ExactBinding);
  }

  public static void FastSet<T>(this T entity, string propertyname, object value) where T : class
  {
    var tt = entity.GetType(); // typeof(T) can be dangerous when entity is object
    if (!_setdelegates.ContainsKey(tt))
      _setdelegates.Add(tt, new Dictionary<string, Delegate>());

    if (!_setdelegates[tt].ContainsKey(propertyname))
      _setdelegates[tt].Add(propertyname, tt.GetProperty(propertyname).FastSetDelegate(tt));

    ((Action<object, object>)_setdelegates[tt][propertyname])(entity, value);
  }

  public static object FastGet<T>(this T entity, string propertyname) where T : class
  {
    var tt = entity.GetType(); // typeof(T) can be dangerous when entity is object
    if (!_getdelegates.ContainsKey(tt))
      _getdelegates.Add(tt, new Dictionary<string, Delegate>());

    if (!_getdelegates[tt].ContainsKey(propertyname))
      _getdelegates[tt].Add(propertyname, tt.GetProperty(propertyname).FastGetDelegate(tt));

    return ((Func<object, object>)_getdelegates[tt][propertyname])(entity);
  }

  public static Func<object, object> FastGetDelegate(this PropertyInfo property, Type t)
  {
    var method = property.GetGetMethod(false);
    MethodInfo ghelper = _gethelper.MakeGenericMethod(t, method.ReturnType);
    return (Func<object, object>)ghelper.Invoke(null, new object[] { method });
  }

  public static Action<object, object> FastSetDelegate(this PropertyInfo property, Type t)
  {
    var method = property.GetSetMethod(false);
    MethodInfo shelper = _sethelper.MakeGenericMethod(t, property.PropertyType);
    return (Action<object, object>)shelper.Invoke(null, new object[] { method });
  }

#if WINDOWS_PHONE
  public static Func<object, object> InternalCreateGetDelegate<T, TReturn>(MethodInfo method)
#else
    private static Func<object, object> InternalCreateGetDelegate<T, TReturn>(MethodInfo method)
#endif
 where T : class
  {
    // Convert the slow MethodInfo into a fast, strongly typed, open delegate
    var mdelegate = (Func<T, TReturn>)Delegate.CreateDelegate(typeof(Func<T, TReturn>), method);

    // Now create a more weakly typed delegate which will call the strongly typed one
    // and return a Func delegate using a lambda expression. 
    return (object target) => mdelegate((T)target);
  }

#if WINDOWS_PHONE
  public static Action<object, object> InternalCreateSetDelegate<T, TValue>(MethodInfo method)
#else
  private static Action<object, object> InternalCreateSetDelegate<T, TValue>(MethodInfo method)
#endif
 where T : class
  {
    // Convert the slow MethodInfo into a fast, strongly typed, open delegate
    var mdelegate = (Action<T, TValue>)Delegate.CreateDelegate(typeof(Action<T, TValue>), method);

    // Now create a more weakly typed delegate which will call the strongly typed one
    // and return a Func delegate using a lambda expression. 
    return (object target, object value) => mdelegate((T)target, (TValue)value);
  }
}
