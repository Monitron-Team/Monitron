using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;

using S22.Xmpp.Extensions;
using S22.Xmpp.Im;
using S22.Xmpp.Core;
using S22.Xmpp.Client;

namespace S22.Xmpp
{
    internal class JabberRpc : XmppExtension, IInputFilter<Iq>
    {
        private readonly Dictionary<string, object> rpcServers = new Dictionary<string, object>();
        public JabberRpc(XmppIm im): base (im)
        {
        }

        public override IEnumerable<string> Namespaces
        {
            get
            {
                return new string[] {
                    "jabber:iq:rpc"
                };
            }
        }

        public bool Input(Iq stanza)
        {
            XmlElement query = stanza.Data["query"];
            if (query == null)
            {
                return false;
            }

            if (query.NamespaceURI != "jabber:iq:rpc")
            {
                return false;
            }
            MethodCall methodCall = new MethodCall(query["methodCall"]);
            string interfaceName = methodCall.MethodName.Split('.')[0];
            string methodName = methodCall.MethodName.Split('.')[1];
            object server = null;
            rpcServers.TryGetValue(interfaceName, out server);
            if (server == null)
            {
                im.IqResponse(
                    type: IqType.Error,
                    id: stanza.Id,
                    to: stanza.From,
                    from: im.Jid,
                    data: new XmppError(ErrorType.Cancel, ErrorCondition.ItemNotFound).Data
                );
            }

            MethodInfo methodInfo = server.GetType().GetMethod(methodName);
            if (methodInfo == null)
            {
                if (server == null)
                {
                    im.IqResponse(
                        type: IqType.Error,
                        id: stanza.Id,
                        to: stanza.From,
                        from: im.Jid,
                        data: new XmppError(ErrorType.Cancel, ErrorCondition.ItemNotFound).Data
                    );
                }
            }
            try
            {
                object[] args = methodCall.ParamsList.GetArgsForMethod(methodInfo);
                object result = methodInfo.Invoke(server, args);
                MethodResponse response = new MethodResponse();
                if (methodInfo.ReturnType != typeof(void))
                {
                    response.AddParam(result);
                }

                im.IqResponse(
                    type: IqType.Result,
                    id: stanza.Id,
                    to: stanza.From,
                    from: im.Jid,
                    data: Xml.Element("query", "jabber:iq:rpc").Child(
                        response.ToXmlElement()
                    )
                );
            }
            catch {
                im.IqResponse(
                    type: IqType.Error,
                    id: stanza.Id,
                    to: stanza.From,
                    from: im.Jid,
                    data: new XmppError(ErrorType.Cancel, ErrorCondition.InternalServerError).Data
                );
            }
            return true;
        }

        public override Extension Xep
        {
            get
            {
                return Extension.JabberRpc;
            }
        }

        public void RegisterJabberRpcServer<I, T>(T server)
            where I : class
            where T : I
        
        {
            rpcServers.Add(typeof(I).Name, server);
        }

        public T CreateRpcClient<T>(Jid target) where T : class
        {
            AppDomain ad = AppDomain.CurrentDomain;
            AssemblyName am = new AssemblyName();
            am.Name = string.Format("{0}Client", typeof(T).Name);
            AssemblyBuilder ab = ad.DefineDynamicAssembly(am, AssemblyBuilderAccess.Run);
            ModuleBuilder mb = ab.DefineDynamicModule("JabberRpc");
            TypeBuilder tb = mb.DefineType(
                am.Name,
                TypeAttributes.Public,
                typeof(JabberRpcClient),
                new Type[] {typeof(T)}
            );
            ConstructorBuilder ctorBuilder = tb.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new Type[] { typeof(XmppIm), typeof(Jid) }
            );
            // Generate constructor
            var gen = ctorBuilder.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Ldarg_2);
            gen.Emit(OpCodes.Call, typeof(JabberRpcClient).GetConstructors()[0]);
            gen.Emit(OpCodes.Ret);
            foreach (var methodInfo in typeof(T).GetMethods())
            {
                MethodBuilder methodBuilder = tb.DefineMethod(
                    methodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                    CallingConventions.Standard
                );
                Type[] parameterTypes = methodInfo.GetParameters().Select(x => x.ParameterType).ToArray();
                methodBuilder.SetParameters(parameterTypes);
                methodBuilder.SetReturnType(methodInfo.ReturnType);
                ILGenerator generator = methodBuilder.GetILGenerator();
                MethodInfo trampolinMethod;
                if (methodInfo.ReturnType == typeof(void))
                {
                    trampolinMethod = typeof(JabberRpcClient)
                        .GetMethod("castMethod", BindingFlags.NonPublic | BindingFlags.Instance);
                }
                else
                {
                    trampolinMethod = typeof(JabberRpcClient)
                    .GetMethod("callMethod", BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(methodInfo.ReturnType);
                }
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Newobj, typeof(MethodCall).GetConstructor(new Type[0]));
                generator.Emit(OpCodes.Dup);
                generator.Emit(OpCodes.Ldstr, string.Format("{0}.{1}", typeof(T).Name, methodInfo.Name));
                generator.Emit(OpCodes.Call, typeof(MethodCall).GetProperty("MethodName").SetMethod);
                for (short i = 0; i < parameterTypes.Length; i++)
                {
                    generator.Emit(OpCodes.Dup);
                    if (parameterTypes[i].IsValueType)
                    {
                        generator.Emit(OpCodes.Ldarg, i + 1);
                        generator.Emit(OpCodes.Box, parameterTypes[i]);
                    }
                    else
                    {
                        generator.Emit(OpCodes.Ldarg, i + 1);
                    }
                    generator.Emit(OpCodes.Call,
                        typeof(MethodCall).GetMethod("AddParam"));
                }

                generator.Emit(
                    OpCodes.Call,
                    trampolinMethod
                );

                generator.Emit(OpCodes.Ret);
                tb.DefineMethodOverride(methodBuilder, methodInfo);
            }

            Type t = tb.CreateType();
            return Activator.CreateInstance(t, new Object[] { im, target }) as T;
        }
    }
}