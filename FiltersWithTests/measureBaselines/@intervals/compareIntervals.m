function stats = compareIntervals(this1, this2)
[thisOut, intervals2Overlap, intervals2NonOverlap overlapI] = ...
    intersect(this1, this2, 1E-9, true);

stats = initEventStats;
stats.nRefIntervals = size(this1);
disp(sprintf('Number of reference events: %d', stats.nRefIntervals));
stats.nDecisionIntervals = size(this2);
disp(sprintf('Number of decision events: %d', stats.nDecisionIntervals));
stats.sens = size(thisOut)/stats.nRefIntervals;
stats.ppv = size(intervals2Overlap)/stats.nDecisionIntervals;
disp(sprintf('Event sens: %5.1f%%, ppv: %5.1f%%', stats.sens*100, stats.ppv*100));
return

