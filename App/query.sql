SELECT
    d.codi,
    d.exam,
    MAX(CASE WHEN d.perf = :exam_1   THEN d.FECHAUTO END) AS fech_perf_1,
    MAX(CASE WHEN d.perf = :exam_2   THEN d.FECHAUTO END) AS fech_perf_2,
    MAX(CASE WHEN d.exam = :exam_1   THEN d.FECHAUTO END) AS fech_exam_1,
    MAX(CASE WHEN d.exam = :exam_2   THEN d.FECHAUTO END) AS fech_exam_2
FROM
    lis.dresu d
WHERE
    (d.perf IN (:exam_1, :exam_2) OR d.exam IN (:exam_1, :exam_2))
    AND d.FECHAUTO >= :fecha_ini
    AND d.FECHAUTO < :fecha_fin
    AND d.codi IN (
        SELECT codi FROM lis.dresu
        WHERE (perf = :exam_1 OR exam = :exam_1)
          AND FECHAUTO >= :fecha_ini
          AND FECHAUTO < :fecha_fin
        INTERSECT
        SELECT codi FROM lis.dresu
        WHERE (perf = :exam_2 OR exam = :exam_2)
          AND FECHAUTO >= :fecha_ini
          AND FECHAUTO < :fecha_fin
    )
GROUP BY
    d.codi, d.exam
ORDER BY
    d.codi, d.exam;