/**
 * 系統連動下拉選單
 *
 * 使用範例：
 *
 *     @example
 *     var factory = Ext.create('MMIS.form.field.SystemCombosFactory');
 *     var combos = factory.generate();
 *     Ext.create('Ext.Viewport',{
 *         items: [
 *             combos["SYS_ID"],
 *             combos["DEV_ID"], 
 *             combos["ASS_ID"],
 *             combos["ITEM_ID"]
 *         ],
 *         renderTo: Ext.getBody()
 *     });
 *
 */
Ext.define('MMIS.form.field.SystemCombosFactory', {
    extend: 'MMIS.form.field.LinkedCombosFactory',

    generate: function (defaultConfig) {
        var config = {
            taskFlow: 'TraTPSystemGetLOOKUP',
            items: [
                {
                    name: 'SYS_ID',
                    fieldLabel: '系統',
                    fields: ['TEXT', 'VALUE'],
                    matchFieldWidth: false,
                    listConfig: { width: 200 }
                },
                {
                    name: 'DEV_ID',
                    fieldLabel: '裝置',
                    fields: ['TEXT', 'VALUE', 'SYS_ID'],
                    parents: [
                        { name: 'SYS_ID', field: 'SYS_ID' }
                    ],
                    matchFieldWidth: false,
                    listConfig: { width: 200 }
                },
                {
                    name: 'ASS_ID',
                    fieldLabel: '配件',
                    fields: ['TEXT', 'VALUE', 'SYS_ID', 'DEV_ID'],
                    parents: [
                        { name: 'SYS_ID', field: 'SYS_ID' },
                        { name: 'DEV_ID', field: 'DEV_ID' }
                    ],
                    matchFieldWidth: false,
                    listConfig: { width: 200 }
                },
                {
                    name: 'ITEM_ID',
                    fieldLabel: '零組件',
                    fields: ['TEXT', 'VALUE', 'SYS_ID', 'DEV_ID', 'ASS_ID'],
                    parents: [
                        { name: 'SYS_ID', field: 'SYS_ID' },
                        { name: 'DEV_ID', field: 'DEV_ID' },
                        { name: 'ASS_ID', field: 'ASS_ID' }
                    ],
                    matchFieldWidth: false,
                    listConfig: { width: 200 }
                }
            ]
        };

        return this.callParent([config, defaultConfig]);
    }

});