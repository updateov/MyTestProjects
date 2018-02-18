%---{{{ function repair()
function [fhr_0, repairIntervals] = repair(fhr_in)
% Function proc_fhr() filters the signal fhr_in using a nonlinear
% transformation in such a way that it removes and approximates empty
% parts of an fhr signal.

c = getRepairConfig;

%---{{{ artifact definitions
% diff_max_in = 20; % ce - 20 was too stringent
% % diff_max_in   = 20;  %--- maximum acceptable fhr difference  between two consequtive
%                      %    points (max fhr jump)
%                      %    defines the beginning of an artifact
% 
% max_pos_slope_orig = 1;   %--- maximum acceptable positive fhr slope
% max_neg_slope_orig = 1;   %--- maximum acceptable negative fhr slope
% 
% min_fhr_val_orig   = 60;  %--- mnimum value of a fhr  *** CHANGED Nov 1, 2005 to fix bug **** 
% max_fhr_val_orig   = 190; %--- maximum value of a fhr

fhr_len = size(fhr_in,2);

diff_fhr=diff(fhr_in);

repairIntervals = fhrPartSet; % set of repairInterval objects

inside=0;         %---  variable shows if we are "inside of an artifact"
i=1;
dropout = 0; % ce - add to flag dropout to 0
failSlope = 0;
lastRepairStart = 0;
lastGoodFhr = (c.MAX_FHR_VAL + c.MIN_FHR_VAL) / 2;
while i < (fhr_len-2)
    if i == 29900
        i
    end
%     
    if inside == 0 % in good signal
        if (fhr_in(i+1) == 0)
            dropout = 1; % found dropout
            artifact = 1;
%             line_begin = i;
            line_begin = findTrueDropoutStart(fhr_in, i, c);
        elseif abs(diff_fhr(i)) > c.DIFF_MAX_IN
            line_begin = getTrueStart(fhr_in, i, c); % see if artifact really starts a bit earlier
            if (verifyArtifact(diff_fhr, line_begin, c)) % ensure not just abrupt decel
                
                if (line_begin <= lastRepairStart) 
                    line_begin = lastRepairStart + 1; % don't try to repair from same spot twice
                end
                lastRepairStart = line_begin;
            
%                 diff_sign=sign(diff_fhr(line_begin));
                jumpIn = diff_fhr(line_begin);
                artifact = 1;
            else
                artifact = 0;
            end
            
        else
            artifact = 0;
        end
        if artifact
            beforeLastGoodFhr = lastGoodFhr;
            if line_begin > 1 % for tracings zero-padded at beginning
                lastGoodFhr = fhr_in(line_begin);
            end
            % adjust maximum / minimum allowable fhr values after dropout
            % based on last valid value
            max_fhr_val = max(c.MAX_FHR_VAL, max(beforeLastGoodFhr, lastGoodFhr) + c.MAX_DIST_FROM_LAST_GOOD);
            min_fhr_val = min(c.MIN_FHR_VAL, min(beforeLastGoodFhr, lastGoodFhr) - c.MAX_DIST_FROM_LAST_GOOD);
            inside = 1;    
            currSlope = getCurrSlope(fhr_in, line_begin - 1, c.SLOPE_MEASURE_TIME); % current avg. slope over last 5 seconds
        end
    else %%%%%%%%%%%%%%%%% in an artifact region %%%%%%%%%%%%%%%%
%         if line_begin == 10132
%             line_begin
%         end
        checkCond = 0;
        %%%%%%%%%%%%%%%%%% DROPOUT %%%%%%%%%%%%%%%%%%%%%%
        if (dropout) % in dropout portion
            checkCond = (fhr_in(i+1) > min_fhr_val ) & (fhr_in(i+2) > min_fhr_val ) ...
                & (fhr_in(i+1) < max_fhr_val ) & (fhr_in(i+2) < max_fhr_val );
            
            if checkCond % first check that have at least 5 seconds of signal before next dropout
                 repairAccepted = 0;
                 if (~(failSlope)) % so don't have to redo this loop if failing slope criteria
%                     dropoutLength = getDropoutLength(fhr_in, line_begin, c.MAX_IGNORE_DROPOUT_LENGTH);
                    
               
                    i = findTrueReturnFromDropout(fhr_in, i, c);
                    dropoutLength = i - line_begin - 1;
                    sigTime = getReturnChunkLength(fhr_in, i, c.RECOVERY_TIME_LOOKAHEAD);
                
                    if (dropoutLength <= c.MAX_IGNORE_DROPOUT_LENGTH)
                        if (abs(fhr_in(i+1) - lastGoodFhr) < c.MAX_FHR_DIFF_ALWAYS_IGNORE_DROP)
                            repairAccepted = 1;
                            line_end = i + 1;
                        end
                    end
                    if (~(repairAccepted) && (sigTime < (c.MIN_RETURN_FROM_DROPOUT))) % less than 5 seconds
                        checkCond = 0;
                        i = i + sigTime; % skip ahead
                    end
                    if (~(repairAccepted) && checkCond && ...
                            (sigTime < min((2 * c.MIN_RETURN_FROM_DROPOUT), (0.5 * dropoutLength)))) % less than 10 seconds, less than half of dropout length
                        % get rid of short chunks after long dropouts that
                        % have significantly different fhr than last good
                        % value
                        if (abs(lastGoodFhr - fhr_in(i+1)) > c.MAX_FHR_DIFF_IGNORE_DROPOUT)
                            checkCond = 0;
                            i = i + sigTime; % skip ahead
                        end
                    end
                 else
                     sigTime = sigTime - 1; % failed slope at last point - reduce sigTime by 1
                     dropoutLength = dropoutLength + 1; % increase dropout length by 1;
                 end
            end
            
            % if dropout is less than 2 seconds and return from dropout
            % less than 20 bpm from last good value then just connect
            if (checkCond && (~(repairAccepted)))
                if (dropoutLength <= c.MAX_IGNORE_DROPOUT_LENGTH)
                    if (abs(fhr_in(i+1) - lastGoodFhr) < c.MAX_FHR_DIFF_IGNORE_DROPOUT) % can have larger diff if return segment > 5 sec
                        repairAccepted = 1;
                        line_end = i + 1;
                    end
                end
            end
            
                       
            if (checkCond && (~(repairAccepted))) % ce - check slope now and just continue to next sample if too steep
                max_pos_slope = max(c.MIN_POS_SLOPE, c.MAX_POS_SLOPE + c.SLOPE_MULT_FACTOR * currSlope); % parametrized by current slope
                max_neg_slope = max(c.MIN_NEG_SLOPE, c.MAX_NEG_SLOPE - c.SLOPE_MULT_FACTOR * currSlope);
                line_end = i+1;
                slope_jump = ( fhr_in(line_end) - fhr_in(line_begin)) / (line_end - line_begin);
                if  (slope_jump > max_pos_slope) | (slope_jump < (- max_neg_slope))
                    checkCond = 0;
                    failSlope = 1; % used to avoid repeat calc on next sample
                    % look for dropout again - if another dropout within 15
                    % seconds then ignore rest of segment
                    if sigTime < (c.MIN_RETURN_WITH_INIT_SLOPE_FAIL) % less than 15 seconds - give up now
                        i = i + sigTime;
                        failSlope = 0; % skipping ahead so reset flag
%                     else
%                         i
                    end
                else % passed slope test on way down - now check exit
                    if sigTime < (c.MIN_RETURN_IGNORE_EXIT_SLOPE) % less than 20 second block before next dropout
                        nextDrop = i + sigTime - 1;
                        if abs(lastGoodFhr - fhr_in(nextDrop-1)) > c.MAX_DIFF_IGNORE_EXIT_SLOPE
                            timeLook = c.MAX_GAP_AFTER_RETURN_FOR_EXIT_SLOPE_CHECK; % look for valid signal within 5 seconds after back to dropout
                        
                            endIndex = min(fhr_len - 1, nextDrop + timeLook);
                            for k = nextDrop:endIndex
                                if (validSignalForInterval(fhr_in, k, min(fhr_len, k + c.MIN_RETURN_FROM_DROPOUT), lastGoodFhr, c.MAX_DIFF_FROM_LAST_GOOD_FOR_EXIT_BLOCK))
%                                 if (fhr_in(k) > 0) && (fhr_in(k+1) > 0) % back to non-dropout
                                    slope_jump = (fhr_in(k+1) - fhr_in(nextDrop-1)) / (k + 1 - nextDrop + 1);
                                    max_pos_slope = max(c.MIN_POS_SLOPE, c.MAX_POS_SLOPE - c.SLOPE_MULT_FACTOR * currSlope); % invert entry slope
                                    max_neg_slope = max(c.MIN_NEG_SLOPE, c.MAX_NEG_SLOPE + c.SLOPE_MULT_FACTOR * currSlope);
                             
                                    if  (slope_jump > max_pos_slope) | (slope_jump < (- max_neg_slope))
                                        checkCond = 0;
                                        i = nextDrop;
                                    end
                                    break;
                                end
                            end
                        end
                    end
                    
                end
                
                   
       
            end
            
        %%%%%%%%%%%%%%%%%%%%%%% OTHER ARTIFACT %%%%%%%%%%%%%%%%%%%%%%%%%%%%    
        else % in other artifact region
            %1) look for short spike < 15 seconds
            timeLook = c.MAX_ARTIFACT_LENGTH;
            foundDropout = 0;
            minDiff = inf;
            minSlopeDiff = inf;
            extrapDiff = 0;
            currDiff = jumpIn;
            endIndex = min(fhr_len - 1, line_begin + timeLook);
            for k = line_begin + 1:endIndex
                extrapDiff = extrapDiff + currSlope;
                currDiff = currDiff + diff_fhr(k);
                if (fhr_in(k+1) == 0) % dropout occurs - can keep going but flag it 
                    foundDropout = 1;
                    dropoutIndex = k;
                end
                
                % use abs diff (w/o slope) as a safeguard in case slope
                % estimate is bad
                if abs(currDiff) < minDiff
                    minIndex = k;
                    minDiff = abs(currDiff);
                end
                
                if (k - line_begin < c.MAX_SHORT_REPAIR) && (abs(currDiff) < (c.MAX_REPAIR_ABS_DIFF / 2)) % favour short repair
                    minIndex = k;
                    minDiff = abs(currDiff);
                    break;
                end
               
                if abs(currDiff - extrapDiff) < minSlopeDiff
                    % for extrap differences only consider signal within
                    % range (as safeguard) - if is truly valid signal then will still get
                    % picked up by absolute diff
                    if ((fhr_in(k+1) < c.MAX_FHR_VAL) && (fhr_in(k+1) > c.MIN_FHR_VAL))
                        minSlopeIndex = k;
                    	minSlopeDiff = abs(currDiff - extrapDiff);
                    end
                end 
            end
            
            % prefer fit w/ slope factored in, but have absolute as backup
            if (    (minSlopeDiff < c.MAX_REPAIR_SLOPE_DIFF) && ...
                    (minSlopeDiff < c.MAX_FACTOR_SLOPE_OVER_ABS * (minDiff + 0.01)) && ...  % + 0.1 just so if minDiff = 0 and minSlopeDiff = 1e-15
                    (fhr_in(minSlopeIndex + 1) > 0))      
                checkCond = 1;
                line_end = minSlopeIndex + 1;             
%                 i = minSlopeIndex; % skip forward in loop
            elseif minDiff < c.MAX_REPAIR_ABS_DIFF
                checkCond = 1;
                line_end = minIndex + 1;
%                 i = minIndex;
            elseif foundDropout % treat as dropout
                dropout = 1;
                i = dropoutIndex; 
            elseif minSlopeIndex > i % for now just take best slopeDiff estimate unless come up with better solution
                checkCond = 1;
                line_end = minSlopeIndex + 1;
%                 i = minSlopeIndex;
            elseif minIndex > i % take minimum absolute difference regardless
                checkCond = 1;
                line_end = minIndex + 1;
                inside = 0; 
            else % 
                inside = 0; % just move on for now
            end
            %2) other criteria
            
                 
        end
        if checkCond  %%%%%%%%% DO ACTUAL REPAIR %%%%%%%%%%%%%%%
            if line_begin == 1 % dropout at start of tracing
                fhr_in(line_begin:line_end) = fhr_in(line_end);
                repairIntervals = add(repairIntervals, repairInterval(intervals(line_begin, line_end - 1)));
            else
                fhr_in(line_begin:line_end)=lin_approx(fhr_in(line_begin:line_end));
                repairIntervals = add(repairIntervals, repairInterval(intervals(line_begin + 1, line_end - 1)));
            end
            
            inside = 0;
            dropout = 0;
            failSlope = 0;
            i = line_end - 1;
        end
        
     end
     
        
      
                

    i = i+1;
  
end

% if at end and still in an artifact region, repeat last sample until end
if inside == 1
    line_end = length(fhr_in);
    fhr_in(line_begin:line_end) = fhr_in(line_begin);
    repairIntervals = add(repairIntervals, repairInterval(intervals(line_begin + 1, line_end)));
end
fhr_0 = fhr_in;

return
%---}}} function repair()

function s = getCurrSlope(fhr, index, timeBack)

    MAX_ABS_SLOPE = 1;
    
    x2 = index;
    while ((x2 > 0) && (abs(fhr(x2 + 1) - fhr(x2)) > 5)) % to ensure no artifact is considered in slope
        x2 = x2 - 1;
    end
    if (index - x2) > 10
        s = 0;
    elseif x2 < timeBack
        s = 0;
    else
        x1 = max(1, x2 - timeBack);
        s = (fhr(x2) - fhr(x1))/(x2-x1);
        if (s > MAX_ABS_SLOPE)
            s = MAX_ABS_SLOPE;
        elseif (s < -MAX_ABS_SLOPE)
            s = -MAX_ABS_SLOPE;
        end
    end
    
return;
 
function i = getTrueStart(fhr, index, c)

    numConsecOK = 0;
    k = index - 1;
    i = index;
    while ((k > 0) && (numConsecOK <= c.ARTIFACT_REWIND_MAX_CONSEC_OK))
        diff_fhr = abs(fhr(k + 1) - fhr(k)); 
        if diff_fhr < c.ARTIFACT_REWIND_MIN_FHR_DIFF
            numConsecOK = numConsecOK + 1;
        elseif diff_fhr > c.ARTIFACT_REWIND_FHR_DIFF
            i = k;
            numConsecOK = 0;
        else
            numConsecOK = 0;
        end
        k = k - 1;
    end
return;
 
function bRC = verifyArtifact(diff_fhr, i, c)

    % If jump in is < 30, and the exit is fairly gradual (no jumps > 15)
    % then consider it a non-artifcact
    bRC = true;
    if abs(diff_fhr(i)) > c.MAX_NON_ARTIFACT_DIFF
        return;
    end
    
    diff_sign = sign(diff_fhr(i));
    
    if diff_sign == 1 % extra check for upward spike because more likely to be artifact
        if abs(sum(diff_fhr(i:i+10))) > 2 * c.MAX_NON_ARTIFACT_DIFF
            return;
        end
    end
    
%     if diff_sign == 1 % if upward spike consider artifact
%         return;
%     end
    
    timeLook = c.MAX_LENGTH_SMALL_JUMP_ARTIFACT;
    endIndex = min(length(diff_fhr), i + timeLook);
    
    if diff_sign == 1 % upward spike
        jumpBack = find(diff_fhr(i+1:endIndex) < -c.MIN_JUMP_BACK_NON_ARTIFACT);
    else
        jumpBack = find(diff_fhr(i+1:endIndex) > c.MIN_JUMP_BACK_NON_ARTIFACT);
    end

    
    if isempty(jumpBack)
        bRC = false;  % empty jump back means stays down or gradual return
    else 
        bRC = true;  % if jumps more than 5 seconds apart then consider not artifact
    end
%     if max(abs(diff_fhr(i+1:endIndex))) < 15
%         bRC = false;
%     end
    
return;
 
 
function bRC = validSignalForInterval(fhr, x1, x2, ref, maxDiff)

    for i = x1:x2
        if (abs(fhr(i) - ref) > maxDiff)
            bRC = 0;
            return;
        end
    end
    
    bRC = 1;
    
return;

function i = findTrueReturnFromDropout(fhr, i, c)

    numConsecOK = 0;
    k = i;
    while ((k < length(fhr)) && (numConsecOK <= c.ARTIFACT_REWIND_MAX_CONSEC_OK))
        diff_fhr = abs(fhr(k + 1) - fhr(k)); 
        if diff_fhr < c.DROPOUT_RETURN_MAX_DIFF
            numConsecOK = numConsecOK + 1;
        else
            i = k;
            numConsecOK = 0;
        end
        k = k + 1;
    end
return;

function i = findTrueDropoutStart(fhr, i, c)

    numConsecOK = 0;
    k = i - 1;
    while ((k > 0) && (numConsecOK <= c.ARTIFACT_REWIND_MAX_CONSEC_OK))
        diff_fhr = abs(fhr(k+1) - fhr(k));
        if diff_fhr < c.DROPOUT_ENTRY_MAX_DIFF
            numConsecOK = numConsecOK + 1;
        else
            i = k;
            numConsecOK = 0;
        end
        k = k - 1;
    end
return;
    
function sigTime = getReturnChunkLength(fhr_in, i, timeLook)

    

    endIndex = min(length(fhr_in), i + timeLook);
    sigTime = inf;
    for k = i+1:endIndex
        if fhr_in(k) == 0
            sigTime = k - i + 1;                 
            break;
        end
    end
    
return;

function t = getDropoutLength(fhr_in, i, timeLook)

    t = 1;
    while (fhr_in(t + i) == 0) && (t <= timeLook)
        t = t + 1;
    end
    
return;
        

    
            

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%%%%%%%%%%%%%%%      Other supporting functions      %%%%%%%%%%%%%%%%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

%---{{{ function lin_approx()
function out_vect = lin_approx(in_vect)
% Function lin_approx() approximates input vector using
% a linear approximation method.

vect_len=size(in_vect, 2);
out_vect = [1:vect_len]*((in_vect(vect_len)-in_vect(1))/(vect_len-1));
first_element = out_vect(1);
out_vect = out_vect + ( in_vect(1) - first_element);

return;
%---}}} function lin_approx()

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

