using System.Collections.Generic;

namespace IxMilia.Iges.Entities
{
    /// <summary>
    /// Represents IGES type 504 Group entity.
    /// </summary>
    public class IgesGroup : IgesEntity
    {
        public override IgesEntityType EntityType => IgesEntityType.Group;
        public List<IgesEntity> AssociatedEntities { get; } = new List<IgesEntity>();

        public IgesGroup() : base() { }

        public IgesGroup(IEnumerable<IgesEntity> associatedEntities)
            : base()
        {
            AssociatedEntities.AddRange(associatedEntities);
        }

        internal static IgesGroup FromParameters(List<string> parameters, IgesDirectoryData directoryData, IgesReaderBinder binder)
        {
            int count = int.Parse(parameters[0]);
            var group = new IgesGroup();
            int found = 0;
            // Start at 1 (skip count), try all remaining parameters as entity indices until we have 'count' entities
            for (int i = 1; i < parameters.Count && found < count; i++)
            {
                if (int.TryParse(parameters[i], out int deIndex))
                {
                    binder.BindEntity(deIndex, entity =>
                    {
                        if (entity != null)
                        {
                            group.AssociatedEntities.Add(entity);
                            found++;
                        }
                    });
                }
            }
            return group;
        }

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            // This method is not used for group parsing; handled by FromParameters
            return parameters.Count;
        }

        internal override void WriteParameters(List<object?> parameters, IgesWriterBinder binder)
        {
            parameters.Add(AssociatedEntities.Count);
            foreach (var entity in AssociatedEntities)
            {
                parameters.Add(binder.GetEntityId(entity));
            }
        }
    }
}