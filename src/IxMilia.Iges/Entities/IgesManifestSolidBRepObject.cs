using System.Collections.Generic;

namespace IxMilia.Iges.Entities
{
    public class IgesManifestSolidBRepVoid
    {
        public IgesShell? Shell { get; set; }
        public bool IsOriented { get; set; }

        public IgesManifestSolidBRepVoid(IgesShell? shell, bool isOriented)
        {
            Shell = shell;
            IsOriented = isOriented;
        }
    }

    public class IgesManifestSolidBRepObject : IgesEntity
    {
        public override IgesEntityType EntityType { get { return IgesEntityType.ManifestSolidBRepObject; } }

        public IgesShell? Shell { get; set; } // Use IgesShell for type safety
        public bool IsOriented { get; set; }
        public List<IgesManifestSolidBRepVoid> Voids { get; } = new List<IgesManifestSolidBRepVoid>();

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            var index = 0;
            int shellPointer = Integer(parameters, index++); // IGES pointers are directory sequence numbers (1-based)
            binder.BindEntity(shellPointer, shell => {
                if (shell is IgesShell s)
                {
                    Shell = s;
                    System.Console.WriteLine($"[IGESMANIFESTSOLID] Bound shell: {s.EntityLabel} SeqNum={shellPointer}");
                }
                else if (shell != null)
                {
                    System.Console.WriteLine($"[IGESMANIFESTSOLID] Pointer {shellPointer} resolved to non-shell: {shell.GetType().Name}");
                }
                else
                {
                    System.Console.WriteLine($"[IGESMANIFESTSOLID] Pointer {shellPointer} did not resolve to any entity.");
                }
            });
            IsOriented = Boolean(parameters, index++);
            var voidCount = Integer(parameters, index++);
            for (int i = 0; i < voidCount; i++)
            {
                var pointer = Integer(parameters, index++);
                var orientation = Boolean(parameters, index++);
                var vd = new IgesManifestSolidBRepVoid(null, orientation);
                Voids.Add(vd);
                binder.BindEntity(pointer, v => {
                    if (v is IgesShell s)
                    {
                        vd.Shell = s;
                        System.Console.WriteLine($"[IGESMANIFESTSOLID] Bound void shell: {s.EntityLabel} SeqNum={pointer}");
                    }
                    else if (v != null)
                    {
                        System.Console.WriteLine($"[IGESMANIFESTSOLID] Pointer {pointer} resolved to non-shell: {v.GetType().Name}");
                    }
                    else
                    {
                        System.Console.WriteLine($"[IGESMANIFESTSOLID] Pointer {pointer} did not resolve to any entity.");
                    }
                });
            }
            return index;
        }

        internal override IEnumerable<IgesEntity?> GetReferencedEntities()
        {
            yield return Shell;
            foreach (var v in Voids)
            {
                yield return v.Shell;
            }
        }

        internal override void WriteParameters(List<object?> parameters, IgesWriterBinder binder)
        {
            parameters.Add(binder.GetEntityId(Shell));
            parameters.Add(IsOriented);
            parameters.Add(Voids.Count);
            foreach (var v in Voids)
            {
                parameters.Add(binder.GetEntityId(v.Shell));
                parameters.Add(v.IsOriented);
            }
        }
    }
}





