function s = compare(setA, setB)

s.TP = 0;
s.FP = 0;
s.DETECTED = 0;
s.MISSED = 0;

if ~exist('c', 'var')
    c.MIN_OVERLAP = 1e-6;
    c.SAVE_PARTS = 0;
end


missedParts = fhrPartSet;


expertSamp = 0;
sampOverlap = 0;
sampNonOverlap = 0;

b = 1;
maxB = size(setB);
TP = zeros(1, maxB);
DETECT = zeros(1, size(setA));

% Here 'A' is the template set and 'B' is the test set
for i = 1:size(setA)
    currPart = subset(setA, i);
    x1 = getX1(currPart);
    x2 = getX2(currPart);
    expertSamp = expertSamp + length(currPart);
    inLoop = 1;
    overlap = 0;
    numOverlap = 0;
    while (inLoop)
        while (b <= maxB) && (getX2(subset(setB, b)) < x1)
            b = b + 1;
        end
        if b <= maxB
            partB = subset(setB, b);
            x1b = getX1(partB);
            x2b = getX2(partB);
            if x1b > x2
                overlapPerc = 0;
                inLoop = 0; % no more parts in B could overlap currPart
            elseif (x1b <= x1) && (x2b >= x2)  % CASE 1
                sampOverlap = sampOverlap + length(currPart);
                overlapPerc = length(currPart) / length(partB);
                if overlapPerc > c.MIN_OVERLAP
                    overlap = 1;
                end
                inLoop = 0; % no more parts in B could overlap currPart
            elseif (x1b <= x1) && (x2b < x2) % CASE 2 - starts before expert, ends before expert
                lenOverlap = x2b - x1 + 1;
                sampOverlap = sampOverlap + lenOverlap;
                denom = max(length(currPart), length(partB));
                overlapPerc = lenOverlap / denom;
                inLoop = 1; % next part could also overlap
            elseif (x1b > x1) && (x2b < x2) % CASE 3 - start after, end before
                sampOverlap = sampOverlap + length(partB);
                overlapPerc = length(partB) / length(currPart);
                inLoop = 1;
            elseif (x1b > x1) && (x2b >= x2) % CASE 4 - start after, end after
                lenOverlap = x2 - x1b + 1;
                sampOverlap = sampOverlap + lenOverlap;
                denom = max(length(currPart), length(partB));
                overlapPerc = lenOverlap / denom;
                inLoop = 0;
            end
            if overlapPerc >= c.MIN_OVERLAP
                overlap = 1; % this expert event was not missed
                TP(b) = 1; % test event is true positive
            end
            if inLoop
                b = b + 1;
            end
        else
            inLoop = 0;
        end
    end
    if (~overlap)
        s.MISSED = s.MISSED + 1;
        missedParts = add(missedParts, currPart);
    else
        s.DETECTED = s.DETECTED + 1;
        DETECT(i) = 1;
    end
end

s.TP = sum(TP);
s.FP = maxB - s.TP;
s.sens = s.DETECTED / size(setA);
s.ppv = s.DETECTED / (s.DETECTED + s.FP);
s.TPmask = TP;
s.ExpMask = DETECT;

bSamples = 0;
for i = 1:maxB
    bSamples = bSamples + length(subset(setB, i));
end
s.expertSamp = expertSamp;
s.sampOverlap = sampOverlap;
s.sampNonOverlap = bSamples - sampOverlap;
s.sampSens = s.sampOverlap / s.expertSamp;
s.sampPPV = s.sampOverlap / bSamples;
s.missedParts = missedParts;
if c.SAVE_PARTS
    s.expParts = setA;
    s.testParts = setB;
end
s.MIN_OVERLAP = c.MIN_OVERLAP;


return;












