using System;
using System.Collections.Generic;

namespace Amanda.Data.Structured
{
    public class RandomSelectionFilter:IViewFilter
    {
        Random r = new Random();
        public ChangeOptions Options
        {
            get;
            private set;
        }
        public RandomSelectionFilter()
        {
            this.Options = new ChangeOptions();
            this.Options.Add("SizeOfResultSet", new FilterOption()
            {
                Description = "Number of instances to end up with.",
                Name = "SizeOfResultSet",
                Value = 100
            });
        }

        public InstanceSet Apply(InstanceSet instances)
        {
            var originalValues = instances.Data;
            var originalLabels = instances.Labels;
            int rows = originalValues.Length;
            int resultSetSize = (int)((FilterOption)Options["SizeOfResultSet"]).Value;
            List<object[]> listToReturn = new List<object[]>();
            HashSet<int> rowsAlreadySelected = new HashSet<int>();
            int nextRow = 0;
            do
            {
                do
                {
                    nextRow = r.Next(0, rows);
                } while (rowsAlreadySelected.Contains(nextRow));
                listToReturn.Add(instances.Data[nextRow]);
            } while (listToReturn.Count < resultSetSize);

            return new InstanceSet()
            {
                Data = listToReturn.ToArray(),
                Labels = originalLabels
            };
        }
    }
}
