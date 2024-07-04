/**
 * 車種連動下拉選單
 *
 * 使用範例：
 *
 *     @example
 *     var factory = Ext.create('MMIS.form.field.WareHouseCombosFactory');
 *     var combos = factory.generate();
 *     Ext.create('Ext.Viewport',{
 *         items: [
 *             combos["INVDEPT_NO"],
 *             combos["WH_NO"]
 *         ],
 *         renderTo: Ext.getBody()
 *     });
 *
 */
Ext.define('MMIS.form.field.WareHouseCombosFactory', {
    extend: 'MMIS.form.field.LinkedCombosFactory',

    generate: function (defaultConfig) {
        var config = {
            taskFlow: 'TraWHComboGet',
            items: [
                {
                    name: 'INVDEPT_WHNO',
                    fieldLabel: '存料單位',
                    valueField: 'VALUE',
                    fields: ['VALUE', 'TEXT'],
                    matchFieldWidth: false,
                    listConfig: { width: 200 },
                    plugins: [Ext.create('MMIS.plugin.AutoFilterCombo')],
                    listeners: {
                        blur: function (combo) {
                            if (combo.getValue()) {
                                combo.setValue(combo.getValue().toUpperCase());
                            }
                        }
                    }
                },
                {
                    name: 'WH_NO',
                    fieldLabel: '庫房別',
                    valueField: 'VALUE',
                    fields: ['VALUE', 'TEXT', 'INVDEPT_WHNO'],
                    parents: [
                        { name: 'INVDEPT_WHNO', field: 'INVDEPT_WHNO' }
                    ],
                    matchFieldWidth: false,
                    listConfig: { width: 200 },
                    plugins: [Ext.create('MMIS.plugin.AutoFilterCombo')],
                    listeners: {
                        blur: function (combo) {
                            if (combo.getValue()) {
                                combo.setValue(combo.getValue().toUpperCase());
                            }
                        }
                    }
                }
            ]
        }

        return this.callParent([config, defaultConfig]);
    }

});