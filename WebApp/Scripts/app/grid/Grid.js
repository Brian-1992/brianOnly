/**
 * 
 */
Ext.define('MMIS.grid.Grid', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.mmisgrid',

    requires: [
        'MMIS.data.GridStore',
        'MMIS.grid.PagingToolbar',
        'MMIS.form.QueryForm',
        'MMIS.data.ModelHalper'
    ],

    /**
     * @cfg {String} taskFlow (required)
     */

    /**
     * @cfg {String} model
     */

    /**
     * @cfg {Object} queryFormConfigs QueryForm config. see {@link MMIS.form.QueryForm}
     */

    /**
     * @cfg {Object} toolbarConfig toolbar config. see {@link MMIS.grid.PagingToolbar}
     */

    /**
     * @cfg {Object} toolbarItems toolbar items. see {@link MMIS.grid.PagingToolbar#items}
     */

    /**
    * @cfg {Object} storeConfig store config. see {@link MMIS.data.GridStore}
    */

    /**
    * @cfg {Object} autoLoadData It will apply to {@link MMIS.data.GridStore#autoLoad}
    */
    autoLoadData: true,

    /**
     * @cfg {Object} storePageSize It will apply to {@link MMIS.data.GridStore#pageSize}.
     */
    storePageSize: 20,

    cls: 'T1',

    initComponent: function () {

        if (Ext.typeOf(this.store) === "string") {
            this.store = Ext.data.StoreManager.lookup(this.store || 'ext-empty-store');
        }

        Ext.apply(this, Ext.applyIf(this.initialConfig, this.beforeInitComponent()));

        this.callParent(arguments);

        this.afterInitComponent();
    },

    getPagingToolbar: function () {
        return this.pagingToolbar;
    },

    getQueryForm: function () {
        return this.queryForm;
    },

    beforeInitComponent: function () {
        var store = null;

        if (this.store != null) {
            store = this.store;
            this.model = this.model || this.store.model;
        }
        else {
            store = this.generateStore();
        }

        this.transformColumns();
        this.columns = this.columns || [];

        if (this.columns[0] && this.columns[0].xtype === 'rownumberer') {
            return { store: store };
        }

        if (Ext.typeOf(this.columns) === "array") {
            this.columns.unshift({ xtype: 'rownumberer' });
        } else {
            this.columns.items.unshift({ xtype: 'rownumberer' });
        }

        return { store: store };
    },

    afterInitComponent: function () {
        if (this.queryFormConfigs) {
            if (Ext.typeOf(this.queryFormConfigs) == "array") {
                this.queryFormConfigs = { items: this.queryFormConfigs };
            }
            this.addDocked([this.generateQueryForm(this.queryFormConfigs)]);
        }

        var toolbar = this.generatePagingToolbar();
        this.pagingToolbar = toolbar;
        this.addDocked([toolbar]);
    },

    generateStore: function () {
        var configs = {
            model: this.model,
            sorters: this.sorters,
            taskFlow: this.taskFlow,
            pageSize: this.storePageSize,
            autoLoad: this.autoLoadData
        };

        Ext.apply(configs, this.storeConfig);
       
        return Ext.create('MMIS.data.GridStore', configs);
    },

    transformColumns: function () {
        var me = this;
        columns = me.columns;

        if (Ext.typeOf(columns) != "array" && columns.items) {
            columns = columns.items;
        }

        Ext.each(columns, function (column, index) {
            if (Ext.typeOf(column) == "string") {
                column = { text: MMIS.data.ModelHalper.getDisplayName(me.model, column), dataIndex: column };
                columns[index] = column;
            }
            else {
                if (!column.text && column.xtype !== 'rownumberer') {
                    column.text = MMIS.data.ModelHalper.getDisplayName(me.model, column.dataIndex);
                }
                if (column.xtype === 'numbercolumn') {
                    column = me.addNumberColumnConfig(column);
                }
            }
        });
    },

    addNumberColumnConfig: function (column) {
        Ext.applyIf(column, { align: 'right', format: '0,0' });
    },

    generatePagingToolbar: function () {
        var configs = {
            layout: 'hbox',
            store: this.store,
            items: this.toolbarItems,
            autoScroll: true
        };
        Ext.apply(configs, this.toolbarConfig);
        return Ext.create('MMIS.grid.PagingToolbar', configs);
    },

    generateQueryForm: function (config) {
        var configs = {
            border: false,
            model: this.model,
            store: this.store
        };
        Ext.apply(configs, config);
        this.queryForm = Ext.create('MMIS.form.QueryForm', configs);
        return this.queryForm;
    }
});