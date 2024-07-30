/**
 * 矩陣運算工具
 */
Ext.define('MMIS.Matrix', {
    singleton: true,

    getNewFields: function (oData, columnfield) {
        var fields = [],
            i = 0,
            l = oData.length;

        for (; i < l ; i += 1) {
            Ext.Array.include(fields, oData[i][columnfield].toString());
        }
        return fields;
    },

    equals: function (o1, o2, compareFields) {
        var i = 0, l = compareFields.length, f;
        for (; i < l; i += 1) {
            f = compareFields[i];
            if (o1[f] !== o2[f]) {
                return false;
            }
        }
        return true;
    },

    getEqualRecordFrom: function (oData, record, compareFields) {
        var me = this,
            i = 0,
            l = oData.length,
            r;

        for (; i < l; i += 1) {
            r = oData[i];
            if (me.equals(r, record, compareFields)) {
                return r
            }
        }
    },

    transformData: function (oData, columnField, rowFields, valueField) {
        var me = this,
            outputfields = rowFields.concat(me.getNewFields(oData, columnField)),
            outputData = [];

        Ext.each(oData, function (r) {
            var compareFields = rowFields,
                oRecord;

            if (oRecord = me.getEqualRecordFrom(outputData, r, compareFields)) {
                oRecord[r[columnField]] = r[valueField];
            } else {
                var newRecord = {};
                Ext.each(rowFields, function (field) {
                    newRecord[field] = r[field];
                });
                newRecord[r[columnField]] = r[valueField];
                outputData.push(newRecord);
            }
        });

        return outputData;
    },

    /**
     * 轉換store欄列
     * @param {String} originStore 原始Store
     * @param {String} columnField 即將作為欄的欄位
     * @param {String} rowFields 即將作為列的欄位
     * @param {String} valueField 即將作為值的欄位
     */
    transformStoreData: function (originStore, columnField, rowFields, valueField) {
        var me = this,
            ofields,
            oData = [];

        Ext.each(originStore.getRange(), function (reacord) {
            oData.push(reacord.getData());
        });

        ofields = rowFields.concat(me.getNewFields(oData, columnField));
        oData = me.transformData(oData, columnField, rowFields, valueField);

        return Ext.create('Ext.data.Store', {
            fields: ofields,
            data: oData
        });
    }
});