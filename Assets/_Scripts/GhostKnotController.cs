using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class GhostKnotController : MonoBehaviour
{
    [SerializeField]
    private Spline spline;

    [SerializeField]
    private SplineController splineController;

    //To save the positions of ghost Knots to send to Curve Controller.
    //Just need to find out the best time to add them to this.
    private Dictionary<int, Vector3> ghostPositions = new Dictionary<int, Vector3>();

    private void Start()
    {
        spline = GetComponent<SplineContainer>().Spline;
    }

    // Method to delete knots and reindex the remaining knots
    public void DeleteKnotsAndTrackIndices(
        List<int> indicesToDelete,
        out Dictionary<int, int> newIndexMapping
    )
    {
        // Sort the indices to delete in descending order to safely remove them
        indicesToDelete.Sort((a, b) => b.CompareTo(a));

        //When I'm updating Ghost Markers, I need to clear this dictionary to refresh their indexes.
        ghostPositions.Clear();

        // Dictionary to store the new indices of the remaining knots
        newIndexMapping = new Dictionary<int, int>();

        // Keep track of the new indices of the knots that are not going to be deleted.
        for (int i = 0; i < spline.Count; i++)
        {
            // If the current index is NOT in the delete list, calculate its new index
            if (!indicesToDelete.Contains(i))
            {
                // The new index is reduced by how many knots have been deleted before this index
                int newIndex = i - indicesToDelete.Count(deletedIndex => deletedIndex < i);
                newIndexMapping[i] = newIndex;
            }
        }

        // Remove each knot by its index, starting from the highest index
        foreach (int index in indicesToDelete)
        {
            spline.RemoveAt(index); // Remove the corresponding knot ratio
        }
    }

    // Method to reinsert deleted knots
    public void ReInsertDeletedKnots(List<int> deletedIndices, Dictionary<int, int> newIndexMapping)
    {
        // Iterate over the deleted indices from highest to lowest (to avoid shifting issues)
        deletedIndices.Sort((a, b) => b.CompareTo(a));

        foreach (int deletedIndex in deletedIndices)
        {
            // Find the surrounding knot indices
            int prevIndex = GetPreviousIndex(newIndexMapping, deletedIndex);
            int nextIndex = GetNextIndex(newIndexMapping, deletedIndex);

            if (prevIndex == -1 || nextIndex == -1)
            {
                continue; // Skip if no valid surrounding indices
            }

            // Get the ratio of the surrounding knots
            float prevRatio = splineController.GetKnotRatioInSpline(newIndexMapping[prevIndex]);
            float nextRatio = splineController.GetKnotRatioInSpline(newIndexMapping[nextIndex]);

            // Calculate the new ratio for the deleted knot
            int deletedIndexInterval = nextIndex - prevIndex - (nextIndex - deletedIndex - 1); //This always gives 3 if 2 knots are deleted, even after you calculated one of the ghosts.
            float newRatio =
                (((nextRatio - prevRatio) / deletedIndexInterval) * (deletedIndex - prevIndex))
                + prevRatio;

            // Use the EvaluatePosition method to find the new position for this ratio
            float3 deletedIndexNewPosition = spline.EvaluatePosition(newRatio);

            // Update the Dictionary of Ghost Positions
            ghostPositions[deletedIndex] = deletedIndexNewPosition;

            // Insert the knot back at the correct index
            int newInsertIndex = newIndexMapping[nextIndex];
            spline.Insert(newInsertIndex, deletedIndexNewPosition, TangentMode.AutoSmooth);
        }
    }

    private int GetPreviousIndex(Dictionary<int, int> newIndexMapping, int currentIndex)
    {
        int highestLowerKey = -1; // Initialize to an invalid index

        foreach (var entry in newIndexMapping)
        {
            if (entry.Key < currentIndex && entry.Key > highestLowerKey)
            {
                highestLowerKey = entry.Key; // Update only if it's higher than the previous lower keys
            }
        }

        if (highestLowerKey != -1)
        {
            return highestLowerKey; // Return the value of the highest lower key
        }

        return -1; // No valid previous index
    }

    // Helper method to get the next index in the newIndexMapping
    private int GetNextIndex(Dictionary<int, int> newIndexMapping, int currentIndex)
    {
        int lowestHigherKey = newIndexMapping.Keys.Last(); // Initialize to an invalid index

        foreach (var entry in newIndexMapping)
        {
            if (entry.Key > currentIndex && entry.Key < lowestHigherKey)
            {
                lowestHigherKey = entry.Key;
            }
        }

        if (lowestHigherKey != newIndexMapping.Keys.First()) //spline.Count + 1 is THE PROBLEM!
        {
            return lowestHigherKey; // Return the value of the lowest higer key
        }
        return -1; // No valid next index
    }

    public Dictionary<int, Vector3> GetGhostPositions()
    {
        return ghostPositions;
    }
}
