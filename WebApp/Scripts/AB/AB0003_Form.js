Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.ImageGridField']);

Ext.onReady(function () {
    var T1Set = '';
    var MatclassComboGet = '/api/AA0038/GetMatclassCombo';
    var BaseunitComboGet = '/api/AA0038/GetBaseunitCombo';
    var GetAgennmByAgenno = '/api/BC0002/GetAgennmByAgenno';
    var AgennoComboGet = '/api/AA0038/GetAgennoCombo';
    var FormEditableGet = '/api/AA0038/GetFormEditable';
    var T1Name = "衛材基本檔維護作業";
    var mLabelWidth = 90;

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };

    var docno = Ext.getUrlParam('docno');
    var m_storeid = Ext.getUrlParam('mStoreid');
    var mat_class = Ext.getUrlParam('matClass');

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },        // 進貨單價
            { name: 'AVG_PRICE', type: 'string' },          // 庫存平均單價
            { name: 'INV_QTY', type: 'string' },            // 中央庫房庫存量
            { name: 'AVG_APLQTY', type: 'string' },         // 平均申請數量
            { name: 'TOT_DISTUN', type: 'string' },         // 最小撥補量
            { name: 'APPQTY', type: 'string' },             // 申請數量，申請量必須為最小撥補量的倍數
            { name: 'HIGH_QTY', type: 'string' },           // 月基準量
            { name: 'TOT_APVQTY', type: 'string' },         // 本月累計核撥量
            { name: 'GTAPL_RESON', type: 'string' },        // 超量原因
            { name: 'APLYITEM_NOTE', type: 'string' },      // 超量原因
            { name: 'NEED_REASON', type: 'string' },        // 需填原因(不為空就要填，前端用)
            { name: 'NEED_REASON_SHOW', type: 'string' },        // 需填原因(不為空就要填，前端用)

            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0003_Form/Get',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    docno: docno,
                    m_storeid: m_storeid,
                    mat_class: mat_class,
                    mmcode: T1Query.getForm().findField('P0').getValue(),
                    mmname_c: T1Query.getForm().findField('P1').getValue(),
                    mmname_e: T1Query.getForm().findField('P2').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            },
        }
    });

    // 超量原因選單
    var reasonStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function setComboData() {
        //盤點人員
        Ext.Ajax.request({
            url: '/api/AB0003_Form/GetReasonCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    reasonStore.removeAll();
                    var reasons = data.etts;
                    if (reasons.length > 0)  {                       
                        for (var i = 0; i < reasons.length; i++) {
                            reasonStore.add({ VALUE: reasons[i].VALUE, TEXT: reasons[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    setComboData();

    function T1Load(first) {
        if (first) {
            T1Tool.moveFirst();
        } else {
            T1Store.load({
                params: {
                    start: 0
                }
            });
        }
        
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '加入至申請單',
                id: 'btnDetailAdd',
                name: 'btnDetailAdd',
                handler: function () {
                    var selection = T1Grid.getSelection();

                    var list = [];
                    for (var i = 0; i < selection.length; i++) {
                        if (selection[i].data.WHMM_VALID == 'N') {
                            var msg = '院內碼 ' + selection[i].data.MMCODE + ' 為<span style="color:red">本庫房不可申領品項</span>';

                            Ext.Msg.alert('提醒', msg);
                            return;
                        }
                        if (isNaN(Number(selection[i].data.APPQTY)) || selection[i].data.APPQTY == "") {
                            var msg = '院內碼 ' + selection[i].data.MMCODE + ' <span style="color:red">申請量</span>未輸入';

                            Ext.Msg.alert('提醒', msg);
                            return;
                        }
                        if (selection[i].data.NEED_REASON == 'Y' && selection[i].data.GTAPL_RESON=='') {
                            var msg = '院內碼 ' + selection[i].data.MMCODE + ' <span style="color:red">超量原因</span>未輸入';

                            Ext.Msg.alert('提醒', msg);
                            return;
                        }
                        if (selection[i].data.NEED_REASON == 'Y' && reasonStore.find('VALUE', selection[i].data.GTAPL_RESON) == -1) {
                            var msg = '院內碼 ' + selection[i].data.MMCODE + ' <span style="color:red">超量原因</span>輸入錯誤';

                            Ext.Msg.alert('提醒', msg);
                            return;
                        }

                        var item = {
                            DOCNO: docno,
                            MMCODE: selection[i].data.MMCODE,
                            APPQTY: selection[i].data.APPQTY,
                            GTAPL_RESON: selection[i].data.GTAPL_RESON,
                            APLYITEM_NOTE: selection[i].data.APLYITEM_NOTE,
                        }
                        list.push(item);
                    }

                    detailAdd(list);
                }
            }
        ]
    });
    function detailAdd(list) {
        //var selection = T1Grid.getSelection();

        //var list = [];
        //for (var i = 0; i < selection.length; i++) {
        //    var item = {
        //        DOCNO: docno,
        //        MMCODE: selection[i].data.MMCODE,
        //        APPQTY: selection[i].data.APPQTY,
        //        GTAPL_RESON: selection[i].data.GTAPL_RESON,
        //        APLYITEM_NOTE: selection[i].data.APLYITEM_NOTE,
        //    }
        //    list.push(item);
        //}

        Ext.getCmp('btnDetailAdd').disable();

        Ext.Ajax.request({
            url: '/api/AB0003_Form/InsertDetail',
            method: reqVal_p,
            params: {
                list: Ext.util.JSON.encode(list)
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel("新增成功");
                    T1Load(false);
                    Ext.getCmp('btnDetailAdd').enable();
                } else {
                    Ext.MessageBox.alert('錯誤', data.msg);
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '院內碼',
                    name: 'P0',
                    id: 'P0',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    maxLength: 13,          // 可輸入最大長度為100
                    padding: '0 4 0 4',
                    width: 190
                },
                {
                    xtype: 'textfield',
                    fieldLabel: '中文品名',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    maxLength: 13,          // 可輸入最大長度為100
                    padding: '0 4 0 4',
                    width: 190
                },
                {
                    xtype: 'textfield',
                    fieldLabel: '英文品名',
                    name: 'P2',
                    id: 'P2',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    maxLength: 13,          // 可輸入最大長度為100
                    padding: '0 4 0 4',
                    width: 190
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        T1Load(true);
                        msglabel('');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('');
                    }
                }
            ]
        }]
    });
    var T1CellEditing = Ext.create('Ext.grid.plugin.CellEditing', {
        clicksToEdit: 1,
        autoCancel: true,
        listeners: {
            beforeedit: function (editor, e, eOpts) {
                if (e.field == 'GTAPL_RESON') {
                    if (!e.record.data.APPQTY) {
                        return false;
                    }
                    if (e.record.data.NEED_REASON == null || e.record.data.NEED_REASON == '') {
                        return false;
                    }
                }
                if (e.field == 'APPQTY') {
                    if (e.record.data.NEED_REASON == '') {

                    }
                }

                //return e.record.get("disabled") == false;
            },
            validateedit: function () {

            }
        }
    });
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plugins: [T1CellEditing],
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'SIMPLE',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            }
        }),
        selType: 'checkboxmodel',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            }, {
                text: "",
                dataIndex: 'WHMM_VALID',
                width: 100,
                renderer: function (val, meta, record) {
                    if (record.data.WHMM_VALID == 'N') {
                        return '<span style="color: red">本庫房不可申領</span>';
                    }
                },
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 120
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 120
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 80,
            },
            {
                text: "進貨單價",
                dataIndex: 'M_CONTPRICE',
                width: 100
            },
            {
                text: "基準量",
                dataIndex: 'HIGH_QTY',
                width: 100
            },
            {
                text: "本月累計核撥量",
                dataIndex: 'TOT_APVQTY',
                width: 100
            },
            {
                text: "最小撥補量",
                dataIndex: 'TOT_DISTUN',
                width: 100
            },
            {
                text: "申請數量",
                dataIndex: 'APPQTY',
                width: 100,
                align: 'right',
                style: 'text-align:left; color:red',
                editor: {
                    xtype: 'numberfield',
                    regexText: '只能輸入數字',
                    //regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                    //maskRe: /[0-9]/,
                    //regex: /^([1-9][0-9]*|0)$/,
                    minValue: 1,
                    hideTrigger: true,
                    listeners: {
                        blur: function (self, record, event, eOpts) {
                            
                            var index = -1;
                            var store = T1Grid.getStore().data.items;
                            for (var i = 0; i < store.length; i++) {
                                if (store[i] == T1LastRec) {
                                    index = i;
                                }
                            }

                            T1Grid.getSelectionModel().deselect(index, true);

                            //var reg = new RegExp("^([1-9][0-9]*|0)$");
                            if (isNaN(Number(self.value)) || Number(self.value) == 0) {
                                self.setValue('');
                                T1LastRec.set('APPQTY', '');
                                T1LastRec.set('APPQTY', null);
                                T1LastRec.set('NEED_REASON', null);
                                return;
                            }

                            var minimal = Number(T1LastRec.data.TOT_DISTUN);
                            if (Number(self.value) % minimal != 0) {
                                self.setValue('');
                                T1LastRec.set('APPQTY', '');
                                T1LastRec.set('APPQTY', null);
                                setTimeout(function () {
                                    Ext.MessageBox.alert('提示', '<span style="color:red">申請量必須為最小撥補量的倍數</span>');

                                }, 50)
                                return;
                            }

                            T1LastRec.set('APPQTY', self.value);

                            T1Grid.getSelectionModel().select(index, true);

                            var highqty = 9999;
                            if (T1LastRec.data.HIGH_QTY != null && T1LastRec.data.HIGH_QTY != '') {
                                highqty = Number(T1LastRec.data.HIGH_QTY);
                            }
                            if ((Number(T1LastRec.data.TOT_APVQTY) + Number(T1LastRec.data.APPQTY)) > highqty) {
                                T1Grid.getStore().data.items[index].data.NEED_REASON = 'Y';
                                T1LastRec.set('NEED_REASON_SHOW', '<span style="color:blue">本月累計核撥量加本次申請量超過月基準量，請敘明原因</span>');
                                T1Grid.getStore().data.items[index].data.NEED_REASON_SHOW = '<span style="color:blue">本月累計核撥量加本次申請量超過月基準量，請敘明原因</span>';

                            } else {

                                T1LastRec.set('GTAPL_RESON', '');
                                T1LastRec.set('NEED_REASON', '');

                            }

                            var selection = T1Grid.getSelection();
                            if (selection.length == 0) {
                                Ext.getCmp('btnDetailAdd').disable();
                            }
                        }
                    }
                }
            },
            {
                text: "超量原因",
                dataIndex: 'GTAPL_RESON',
                //flex: 1,
                width: 215,
                editor: {
                    xtype: 'combobox',
                    store: reasonStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    listeners: {
                        blur: function (self, record, event, eOpts) {
                            var index = -1;
                            var store = T1Grid.getStore().data.items;
                            for (var i = 0; i < store.length; i++) {
                                if (store[i] == T1LastRec) {
                                    index = i;
                                }
                            }

                            T1Grid.getSelectionModel().deselect(index, true);

                            T1LastRec.set('GTAPL_RESON', self.value);
                            if (self.value == null) {
                                T1LastRec.set('NEED_REASON_SHOW', '<span style="color:blue">本月累計核撥量加本次申請量超過月基準量，請敘明原因</span>');
                                T1Grid.getStore().data.items[index].data.NEED_REASON_SHOW = '<span style="color:blue">本月累計核撥量加本次申請量超過月基準量，請敘明原因</span>';
                            } else if (reasonStore.find('VALUE', self.value) == -1) {
                                T1LastRec.set('NEED_REASON_SHOW', '<span style="color:blue">本月累計核撥量加本次申請量超過月基準量，請敘明原因</span>');
                                T1Grid.getStore().data.items[index].data.NEED_REASON_SHOW = '<span style="color:blue">本月累計核撥量加本次申請量超過月基準量，請敘明原因</span>';
                            }else {
                                T1LastRec.set('NEED_REASON_SHOW', '');
                                T1Grid.getStore().data.items[index].data.NEED_REASON_SHOW = '';
                            }

                            T1Grid.getSelectionModel().select(index, true);

                            var selection = T1Grid.getSelection();
                            if (selection.length == 0) {
                                Ext.getCmp('btnDetailAdd').disable();
                            }
                        }
                    }
                },
                renderer: function (value) {
                    for (var i = 0; i < reasonStore.data.items.length; i++) {
                        if (reasonStore.data.items[i].data.VALUE == value) {
                            return reasonStore.data.items[i].data.TEXT;
                        }
                    }
                    return '';
                },
                //listeners: {
                //    beforeedit: function (e, editor) {
                //        
                //        if (e.rowIdx == 0)
                //            return false;
                //    }
                //}
            },
            {
                header: "",
                dataIndex:'NEED_REASON_SHOW',
                flex: 1,
                
            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                T1LastRec = record;

            },
            selectionchange: function (model, records) {

                if (records.length == 0) {
                    T1LastRec = null;
                    Ext.getCmp('btnDetailAdd').disable();
                    return;
                }
                Ext.getCmp('btnDetailAdd').enable();
            }
        }
    });

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 'form',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }]
    });
    T1Load(true);
});