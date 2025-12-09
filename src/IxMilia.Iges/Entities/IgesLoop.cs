using System.Collections.Generic;

namespace IxMilia.Iges.Entities
{
    public class IgesLoop : IgesEntity
    {
        public override IgesEntityType EntityType => IgesEntityType.Loop;

        // Added for LaserConvert compatibility
        public List<IgesEntity>? Curves { get; set; }
        public bool IsOuter { get; set; }

        public IgesLoop() {}

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            int index = 0;
            int curveCount = Integer(parameters, index++);
            Curves = new List<IgesEntity>();
            for (int i = 0; i < curveCount; i++)
            {
                int curvePointer = Integer(parameters, index++);
                binder.BindEntity(curvePointer, e => {
                    if (e != null)
                        Curves.Add(e);
                });
            }
            // Read IsOuter flag (0 = inner, 1 = outer)
            IsOuter = Integer(parameters, index++) == 1;
            return index;
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
            // Write IsOuter flag (0 = inner, 1 = outer)
            parameters.Add(IsOuter ? 1 : 0);
        }
    }
}