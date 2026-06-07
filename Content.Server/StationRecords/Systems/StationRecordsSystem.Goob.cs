namespace Content.Server.StationRecords.Systems;

public sealed partial class StationRecordsSystem
{
        private bool IsFilterWithSomeCodeValue(string value, string filter)
    {
        // Goob edit start - Partial Prints Feature
        List<(string, int)> filterletList = ApplyWildcard(filter);

        //NOTE TO SELF: IF TRUE, FILTER THIS ENTRY
        //SECOND NOTE TO SELF: ALL FILTERS NEED TO RETURN TRUE, THEN FINALLY RETURN FALSE
        bool allFiltersPassed = true;
        foreach (var (filterlet, cutoff) in filterletList)
        {
            allFiltersPassed = allFiltersPassed && value.Substring(cutoff).ToLower().StartsWith(filterlet);
        }

        return !allFiltersPassed;

        //OG Logic
        //return !value.ToLower().StartsWith(filter);
    }

    /// <summary>
    /// This helper method chops a filter into a list of filterlets and indexes.
    /// Indexes must be provided because we can only match the start of a string
    /// </summary>
    /// <param name="filter"> The thing to be slam-chopped </param>
    /// <returns>A list of filterlets and the index they come from</returns>
    private List<(string, int)> ApplyWildcard(string filter)
    {
        var filterList = new List<(string, int)>();
        string filterlet = "";
        int segmentStart = 0;
        int index = 0;

        foreach (char c in filter)
        {

            if (c == '#')
            {
                if (!string.IsNullOrEmpty(filterlet)) // The current filterlet string is finished, so-
                {
                    filterList.Add((filterlet, segmentStart)); // -save the filterlet-
                    filterlet = ""; // -and start search for a new one
                }
            }
            else
            {
                if (string.IsNullOrEmpty(filterlet))
                {
                    // This is the start of a new segment
                    segmentStart = index;
                }

                filterlet += c; // ###F##D8
            }

            index++;
        }

        // Don't forget the last segment
        if (!string.IsNullOrEmpty(filterlet))
        {
            filterList.Add((filterlet, segmentStart));
        }

        return filterList;
    } // Good edit end - Partial Prints Feature
}
