using Algocura.ClinicalResearch.Api.Contracts;
using Algocura.ClinicalResearch.Api.Domain;

namespace Algocura.ClinicalResearch.Api.Mapping;

public static class ClinicalResearchMappings
{
    public static ClinicalDatasetResponse ToResponse(this ClinicalDataset dataset, int subjectCount) =>
        new(dataset.Id, dataset.Name, dataset.SourceName, dataset.SourceUri, dataset.StudyCode,
            dataset.DiseaseArea, dataset.Description, dataset.Status, subjectCount,
            dataset.CreatedAtUtc, dataset.UpdatedAtUtc);

    public static ResearchSubjectResponse ToResponse(this ResearchSubject subject) =>
        new(subject.Id, subject.ClinicalDatasetId, subject.ExternalSubjectId, subject.AgeAtDiagnosis,
            subject.Sex, subject.PrimarySite, subject.DiseaseStage, subject.VitalStatus,
            subject.Diagnosis, subject.TreatmentSummary, subject.MolecularFeaturesJson,
            subject.AdditionalAttributesJson, subject.ImportedAtUtc, subject.UpdatedAtUtc);
}
