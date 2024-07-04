/**
 * 故障連動下拉選單
 */
Ext.define('MMIS.form.field.FauCombosFactory', {
    extend: 'MMIS.form.field.LinkedCombosFactory',

    generate: function (defaultConfig) {
        var config = {
            taskFlow: 'TraTPFauGetLOOKUP',
            items: [
                {
                    name: 'TREE_FAU_CLASS',
                    fieldLabel: '故障類別',
                    valueField: 'FAU_CLASS',
                    fields: ['TEXT', 'FAU_CLASS'],
                    matchFieldWidth: false,
                    listConfig: { width: 200 }
                },
                {
                    name: 'TREE_FAU_PROBLEM',
                    fieldLabel: '問題',
                    valueField: 'FAU_PROBLEM',
                    fields: ['TEXT', 'FAU_PROBLEM', 'FAU_CLASS'],
                    parents: [
                        { name: 'TREE_FAU_CLASS', field: 'FAU_CLASS' }
                    ],
                    matchFieldWidth: false,
                    listConfig: { width: 200 }
                },
                {
                    name: 'TREE_FAU_CAUSE',
                    fieldLabel: '原因',
                    valueField: 'FAU_CAUSE',
                    fields: ['TEXT', 'FAU_CAUSE', 'FAU_PROBLEM', 'FAU_CLASS'],
                    parents: [
                        { name: 'TREE_FAU_CLASS', field: 'FAU_CLASS' },
                        { name: 'TREE_FAU_PROBLEM', field: 'FAU_PROBLEM' }
                    ],
                    matchFieldWidth: false,
                    listConfig: { width: 250 }
                }
            ]
        }

        return this.callParent([config, defaultConfig]);
    }

});