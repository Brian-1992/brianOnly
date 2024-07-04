/**
 * 連動下拉選單產生器
 *
 * 使用範例：
 *
 *     @example
 *     var combosFactory = Ext.create('MMIS.form.field.LinkedCombosFactory');
 *     var combos = combosFactory.generate({
 *         taskFlow: '/api/flow/process/TraTPCarGetLOOKUP',
 *         items: [
 *              {
 *                  name: 'CAR_KIND',
 *                  valueField: 'CAR_KIND',
 *                  fieldLabel: '車種',
 *                  fields: ['TEXT', 'CAR_KIND']
 *              },
 *              {
 *                  name: 'CAR_MODEL',
 *                  valueField: 'CAR_MODEL',
 *                  fieldLabel: '型號',
 *                  fields: ['TEXT', 'CAR_MODEL', 'CAR_KIND'],
 *                  parents: [
 *                      { name: 'CAR_KIND', field: 'CAR_KIND' }
 *                  ]
 *              },
 *              {
 *                  name: 'CAR_FUN',
 *                  valueField: 'CAR_FUN',
 *                  fieldLabel: '車廂型式',
 *                  fields: ['TEXT', 'CAR_FUN', 'CAR_KIND', 'CAR_MODEL'],
 *                  parents: [
 *                      { name: 'CAR_KIND', field: 'CAR_KIND' },
 *                      { name: 'CAR_MODEL', field: 'CAR_MODEL' }
 *                  ]
 *              }
 *          ]
 *     });
 *     
 *     Ext.create('Ext.Viewport',{
 *         items: [combos["CAR_KIND"], combos["CAR_MODEL"], combos["CAR_FUN"]],
 *         renderTo: Ext.getBody()
 *     });
 *
 * 使用Ext.create產生factory實體後，呼叫{@link #generate}方法並傳入參數以產生連動下拉選單集合。
 */
Ext.define('MMIS.form.field.LinkedCombosFactory', {

    requires: ['MMIS.form.field.XCombo'],

    /**
     * Creates the factory.
     * @param {Object} [config] Combos's Config.
     */
    constructor: function (config) {
        config = Ext.apply({}, config);
        if (config.items) {
            this.generate(config);
        }
    },

    /**
    * Get the Combo with the specified name.
    * @param {Ext.form.ComboBox} name The name of the Combo to find.
    */
    get: function (name) {
        if (name) {
            return this.combos[name];
        }
    },

    /**
    * Get the All Combos with last generated.
    * @return {Object} the object contain all generated combos use field name as key to get specified combo. ex:combos['CAR_KIND'].
    */
    getAll: function () {
        return this.combos;
    },

    /**
    * 產生連動下拉選單集合。
    * @param {Object} config generate config
    *  @param {Object} config.taskFlow (required) taskFlow位址
    *  @param {Object} config.items (required) combos config
    *   @param {Object[]} config.items.parents 欲連結的上層下拉選單
    *   @param {String} config.items.parents.name 父下拉選單名稱
    *   @param {String} config.items.parents.field 子選單欲連結欄位
    * @param {Object} defaultConfig the config will apply to all combos
    * @return {Object} the object contain all generated combos use field name as key to get specified combo. ex:combos['CAR_KIND'].
    */
    generate: function (config, defaultConfig) {

        if (defaultConfig) {
            this.applyDefaultConfig(config.items, defaultConfig);
        }

        if (config.taskFlow) {
            this.applyTaskFlowConfig(config.items, config.taskFlow);
        }

        var combos = this.generateCombos(config.items);

        this.generateLinks(combos);

        this.combos = combos;

        return combos;
    },

    applyDefaultConfig: function (items, defaultConfig) {
        for (var i = 0; i < items.length; i += 1) {
            Ext.applyIf(items[i], defaultConfig);
        }
    },

    applyTaskFlowConfig: function (items, taskFlow) {
        for (var i = 0; i < items.length; i += 1) {
            Ext.applyIf(items[i], { taskFlow: taskFlow });
        }
    },

    generateCombos: function (configs) {
        var combos = [],
            config, key;
        for (key in configs) {
            config = configs[key];
            combos[config.name] = this.generateCombo(config);
        }
        return combos;
    },

    generateLinks: function (combos) {
        for (var key in combos) {
            this.generateLink(combos[key], combos);
        }
    },

    generateLink: function (targetCombo, combos) {
        var parentCombos = this.getParentCombos(targetCombo, combos);
        this.generateSetParentValueAction(targetCombo, parentCombos);
        this.registParentChangeListener(targetCombo, parentCombos);
    },

    generateSetParentValueAction: function (targetCombo, parentCombos) {
        targetCombo.on("beforeselect", function (combo, record, index, pCombos) {
            var key, linkedField, value;
            for (key in pCombos) {
                linkedField = this.getPraentConfig(combo, key).field;
                value = record.get(linkedField);
                if (pCombos[key].getValue() != value) {
                    pCombos[key].setValue(value);
                }
            }
        }, this, parentCombos);
    },

    registParentChangeListener: function (targetCombo, parentCombos) {
        var me = this,
            onChange = function (combo, newValue, oldValue, pCombos) {
                this.clearValue();
                me.doFilter(this, pCombos);
            },
            key, pCombo;

        for (key in parentCombos) {
            pCombo = parentCombos[key];
            pCombo.on('change', onChange, targetCombo, parentCombos);
        }
    },

    getParentCombos: function (targetCombo, combos) {
        var arrRtn = [],
            parentConfigs = targetCombo.parents,
            i = 0,
            l = parentConfigs ? parentConfigs.length : 0,
            pName;

        for (; i < l; i += 1) {
            pName = parentConfigs[i].name;
            arrRtn[pName] = combos[pName];
        }
        return arrRtn;
    },

    getPraentConfig: function (combo, parentName) {
        var parents = combo.parents, i = 0;
        for (; i < parents.length; i += 1) {
            if (parents[i].name == parentName) {
                return parents[i];
            }
        }
    },

    doFilter: function (targetCombo, pCombos) {
        var tStore = targetCombo.getStore(),
            key, pCombo, targetFieldName, value;

        tStore.clearFilter();
        for (key in pCombos) {
            if (!pCombos[key].getValue()) {
                continue;
            }
            pCombo = pCombos[key];
            targetFieldName = this.getPraentConfig(targetCombo, pCombo.getName()).field;
            value = pCombo.getValue();
            tStore.filter({ property: targetFieldName, value: value, exactMatch: true });
        }
    },

    generateCombo: function (config) {
        var initConfig = {
            queryMode: 'local',
            store: this.generateCombostore(config.name, config.fields, config.taskFlow),
            displayField: 'TEXT',
            valueField: 'VALUE'
        };
        Ext.apply(initConfig, config);
        return Ext.create('MMIS.form.field.XCombo', initConfig);
    },

    generateCombostore: function (name, fields, taskFlow) {
        return Ext.create('MMIS.data.ComboStore', {
            fields: fields,
            autoLoad: true,
            listeners: {
                beforeload: function (store, options) {
                    store.proxy.setExtraParam("x", name);
                }
            },
            taskFlow: taskFlow
        });
    }
});