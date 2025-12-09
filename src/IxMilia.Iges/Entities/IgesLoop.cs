using System.Collections.Generic;

namespace IxMilia.Iges.Entities
{
    public class IgesLoop : IgesEntity
    {
        public override IgesEntityType EntityType => IgesEntityType.Loop;

        public List<IgesEntity>? Curves { get; set; }
        public bool IsOuter { get; set; }

        public IgesLoop() {}

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            int index = 0;
            
            // Plasticity format: surface_ptr, curve_count, curve_ptr(s), ...
            int surfacePtr = Integer(parameters, index++);
            int curveCount = Integer(parameters, index++);
            
            Curves = new List<IgesEntity>();
            
            // Read the specified number of curve pointers
            for (int i = 0; i < curveCount && index < parameters.Count; i++)
            {
                int curvePointer = Integer(parameters, index++);
                binder.BindEntity(curvePointer, e => {
                    if (e != null)
                        Curves.Add(e);
                });
            }
            
            // IsOuter is typically the last parameter
            IsOuter = false;
            if (index < parameters.Count)
            {
                IsOuter = Integer(parameters, index++) == 1;
            }
            
            return parameters.Count;
        }

        internal override void WriteParameters(List<object?> parameters, IgesWriterBinder binder)
        {
            parameters.Add(Curves?.Count ?? 0);
            if (Curves != null)
            {
                foreach (var curve in Curves)
                {
                    parameters.Add(binder.GetEntityId(curve));
                }
            }
            parameters.Add(IsOuter ? 1 : 0);
        }
    }
}