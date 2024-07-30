/**
 * 車種連動下拉選單
 *
 * 使用範例：
 *
 *     @example
 *     var factory = Ext.create('MMIS.form.field.CarCombosFactory');
 *     var combos = factory.generate();
 *     Ext.create('Ext.Viewport',{
 *         items: [
 *             combos["CAR_KIND"],
 *             combos["CAR_MODEL"], 
 *             combos["CAR_FUN"]
 *         ],
 *         renderTo: Ext.getBody()
 *     });
 *
 */
Ext.define('MMIS.form.field.CarCombosFactory', {
    extend: 'MMIS.form.field.LinkedCombosFactory',

    generate: function (defaultConfig) {
        var config = {
            taskFlow: 'TraTPCarGetLOOKUP',
            items: [
                {
                    name: 'CAR_KIND',
                    fieldLabel: '車種',
                    valueField: 'CAR_KIND',
                    fields: ['TEXT', 'CAR_KIND'],
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
                    name: 'CAR_MODEL',
                    fieldLabel: '型號',
                    valueField: 'CAR_MODEL',
                    fields: ['TEXT', 'CAR_MODEL', 'CAR_KIND'],
                    parents: [
                        { name: 'CAR_KIND', field: 'CAR_KIND' }
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
                },
                {
                    name: 'CAR_FUN',
                    fieldLabel: '車廂型式',
                    valueField: 'CAR_FUN',
                    fields: ['TEXT', 'CAR_FUN', 'CAR_MODEL', 'CAR_KIND'],
                    parents: [
                        { name: 'CAR_KIND', field: 'CAR_KIND' },
                        { name: 'CAR_MODEL', field: 'CAR_MODEL' }
                    ],
                    plugins: [Ext.create('MMIS.plugin.AutoFilterCombo')]
                }
            ]
        }

        return this.callParent([config, defaultConfig]);
    }

});