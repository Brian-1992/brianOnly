/**
 * 此類別用以提供取得設定於model中欄位的displayName
 */
Ext.define('MMIS.data.ModelHalper', {

    singleton: true,

    /** */
    getDisplayName: function (model, name) {
        var displayName = "",
            T1Model = Ext.ModelManager.getModel(model),
            modelfields;

        if (!T1Model) {
            throw 'give model is not exist';
        }

        modelfields = T1Model.getFields();

        Ext.each(modelfields, function (field, index) {
            if (field.name == name) {
                displayName = field.displayName ? field.displayName : "";
                return;
            }
        });

        return displayName;
    }
});