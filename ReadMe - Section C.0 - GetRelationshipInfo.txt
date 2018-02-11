C.0 GetRelationshipInfo(actor, relationship, relative)

This method allows direct access to the results of a relationship between and actor and relative.
The function only returns results for actor-relationship-relative combos which are required by
some gesture. Combination that are not used with return float.NAN. A reltionship will have a value
if 1 if it is true or 0 if it is false except for Distance, XChange, YChange and ZChange which will
provide the relationship value instead (i.e. amount of Distance, XChange, YChange or ZChange).

Normally this method does not need to be used. However, it is provided for advanced plugin use or
for assisting in troubleshooting gesture configuration.