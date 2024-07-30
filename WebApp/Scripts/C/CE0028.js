Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    // var T1Get = '/api/CE0028/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "中央庫房盤點駁回";
    var GetCHK_STATUS = '/api/CE0028/GetCHK_STATUS';
    var GetGoBack = '/api/CE0028/GoBack';

    var T1Rec = 0;
    var T1LastRec = null;

    var CHK_STATUS;

    //盤點類別
    var CHK_TYPE_Store = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    //物料類別
    var CHKCLASS_Store = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    //盤點類別清單
    function setCHK_TYPE() {
        CHK_TYPE_Store.add({ VALUE: '0', TEXT: '0 非庫備' });
        CHK_TYPE_Store.add({ VALUE: '1', TEXT: '1 庫備' });
    }

    //物料類別清單
    function setCHKCLASS() {
        CHKCLASS_Store.add({ VALUE: '02', TEXT: '02 衛材' });
        CHKCLASS_Store.add({ VALUE: '07', TEXT: '07 被服' });
        CHKCLASS_Store.add({ VALUE: '08', TEXT: '08 資訊耗材' });
        CHKCLASS_Store.add({ VALUE: '0X', TEXT: '0X 一般物品' });
    }

    //#region CheckboxModel
    Ext.define('overrides.selection.CheckboxModel', {
        override: 'Ext.selection.CheckboxModel',

        getHeaderConfig: function () {
            var config = this.callParent();
            if (Ext.isFunction(this.selectable)) {
                config.selectable = this.selectable;
                config.renderer = function (value, metaData, record, rowIndex, colIndex, store, view) {
                    if (this.selectable(record)) {
                        record.selectable = false;
                        return '';
                    }
                    record.selectable = true;
                    return this.defaultRenderer();
                };
                this.on('beforeselect', function (rowModel, record, index, eOpts) {
                    return !this.selectable(record);
                }, this);
            }
            return config;
        },

        updateHeaderState: function () {
            // check to see if all records are selected
            var me = this,
                store = me.store,
                storeCount = store.getCount(),
                views = me.views,
                hdSelectStatus = false,
                selectedCount = 0,
                selected, len, i, notSelectableRowsCount = 0, temp = 0;


            if (!store.isBufferedStore && storeCount > 0) {
                hdSelectStatus = true;
                store.each(function (record) {
                    if (record.data.STATUS_TOT_A  != '3') {
                        notSelectableRowsCount++;
                    }
                }, this);

                selected = me.selected;

                for (i = 0, len = selected.getCount(); i < len; ++i) {
                    if (store.indexOfId(selected.getAt(i).id) > -1) {
                        ++selectedCount;
                    }
                }
                hdSelectStatus = storeCount === selectedCount + notSelectableRowsCount;
            }

            if (views && views.length) {
                me.column.setHeaderStatus(hdSelectStatus);
            }
        }
    });
    //#endregion

    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right'
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'displayfield',
                    fieldLabel: '庫房代碼',
                    name: 'P0',
                    id: 'P0',
                    value: '560000 中央庫房',
                    labelWidth: 70,
                    width: 180
                },
                {
                    xtype: 'monthfield',
                    fieldLabel: '盤點年月',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,// 限制可輸入最大長度
                    allowBlank: false,
                    autoSelect: true,
                    anyMatch: true,
                    fieldCls: 'required',
                    labelWidth: 70,
                    width: 200
                }, {
                    xtype: 'combo',
                    fieldLabel: '盤點類別',
                    store: CHK_TYPE_Store,
                    name: 'P2',
                    id: 'P2',
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    blankText: "請輸入盤點類別",
                    autoSelect: true,
                    anyMatch: true,
                    allowBlank: false,
                    forceSelection: true,
                    fieldCls: 'required',
                    labelWidth: 70,
                    width: 200,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                }, {
                    xtype: 'combo',
                    fieldLabel: '物料類別',
                    store: CHKCLASS_Store,
                    name: 'P3',
                    id: 'P3',
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    blankText: "請輸入物料類別",
                    autoSelect: true,
                    anyMatch: true,
                    allowBlank: false,
                    forceSelection: true,
                    fieldCls: 'required',
                    labelWidth: 70,
                    width: 200,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                }, {
                    xtype: 'button',
                    text: '查詢',
                    iconCls: 'TRASearch',
                    handler: function () {
                        msglabel("");
                        if (T1Query.getForm().isValid() == false) {
                            T1Store.removeAll();
                            if ((T1Query.getForm().findField("P1").getValue() == null) || (T1Query.getForm().findField("P1").getValue() == "")) {
                                Ext.Msg.alert('提醒', '<span style=\'color:red\'>盤點年月</span>為必填');
                                msglabel(" <span style='color:red'>盤點年月</span>為必填");
                            }
                            else if ((T1Query.getForm().findField("P2").getValue() == null) || (T1Query.getForm().findField("P2").getValue() == "")) {
                                Ext.Msg.alert('提醒', '<span style=\'color:red\'>盤點類別</span>為必填');
                                msglabel(" <span style='color:red'>盤點類別</span>為必填");
                            }
                            else if ((T1Query.getForm().findField("P3").getValue() == null) || (T1Query.getForm().findField("P3").getValue() == "")) {
                                Ext.Msg.alert('提醒', '<span style=\'color:red\'>物料類別</span>為必填');
                                msglabel(" <span style='color:red'>物料類別</span>為必填");
                            }
                            return;
                        }

                        Ext.Ajax.request({
                            url: GetCHK_STATUS,
                            method: reqVal_p,
                            params: {
                                p1: T1Query.getForm().findField('P1').rawValue,
                                p2: T1Query.getForm().findField('P2').getValue(),
                                p3: T1Query.getForm().findField('P3').getValue()
                            },
                            success: function (response) {
                                
                                CHK_STATUS = Ext.decode(response.responseText);
                                if (CHK_STATUS == '2') {
                                    T1Grid.down('#goback').setDisabled(false);
                                    T1Load();
                                }
                                else if (CHK_STATUS == '0') {
                                    Ext.Msg.alert('提醒', '尚未開立三盤單，無法使用駁回功能');
                                    T1Load();
                                    T1Grid.down('#goback').setDisabled(true);
                                } else if (CHK_STATUS == '1') {
                                    Ext.Msg.alert('提醒', '三盤盤點中，無法使用駁回功能');
                                    T1Grid.down('#goback').setDisabled(true);
                                } else if (CHK_STATUS == '3') {
                                    Ext.Msg.alert('提醒', '三盤已完成，無法使用駁回功能');
                                    T1Load();
                                    T1Grid.down('#goback').setDisabled(true);
                                }
                            },
                            failure: function (response, options) {
                            }
                        });
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    iconCls: 'TRAClear',
                    handler: function () {
                        var f = this.up('form').getForm();
                        msglabel("");
                        f.reset();
                        f.findField('P1').focus(); // 進入畫面時輸入游標預設在P0
                        T1Query.getForm().findField('P1').setValue(new Date());
                    }
                }
            ]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.CE0028', { // 定義於/Scripts/app/store/CE0028.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P2的值代入參數
                var np = {
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    //T1Store.on('load', function (store, options) {
        //SetButton();
    //});

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'goback', text: '駁回', disabled: true, handler: function () {
                    goback();
                }
            }
        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'SIMPLE',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return record.data.STATUS_TOT_A != '3';
            }
        }),
        columns: [{
            xtype: 'rownumberer',
            width: 30,
            align: 'Center',
            labelAlign: 'Center'
        },
        {
            text: "院內碼",
            dataIndex: 'MMCODE_A',
            width: 80
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C_A',
            width: 250
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E_A',
            width: 250
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT_A',
            width: 65
        }, {
            text: "電腦量",
            dataIndex: 'STORE_QTY_A',
            width: 80,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0.00'
        }, {
            text: "盤點量",
            dataIndex: 'CHK_QTY_A',
            width: 80,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0.00'
        }, {
            text: "盤差",
            dataIndex: 'GAP_T_A',
            width: 80,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0.00'
        }],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    //SetButton();
                }
            }
        },
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                if (T1LastRec) {
                    msglabel("");
                }
            }
        }
    });

    //控制駁回按鈕是否可按
    function SetButton() {
        Ext.Ajax.request({
            url: GetCHK_STATUS,
            method: reqVal_p,
            params: {
                p1: T1Query.getForm().findField('P1').rawValue,
                p2: T1Query.getForm().findField('P2').getValue(),
                p3: T1Query.getForm().findField('P3').getValue()
            },
            success: function (response) {
                
                CHK_STATUS = response.responseText.replace(new RegExp('"', "g"), '');
                if (CHK_STATUS == '2') {
                    T1Grid.down('#goback').setDisabled(false);
                }
                else {
                    T1Grid.down('#goback').setDisabled(true);
                }
            },
            failure: function (response, options) {
            }
        });
    }

    function goback() {
        var selection = T1Grid.getSelection();
        let CHK_NO_A = '';
        let MMCODE_A = '';
        //selection.map(item => {
        //    CHK_NO_A += item.get('CHK_NO_A') + ',';
        //    MMCODE_A += item.get('MMCODE_A') + ',';
        //});
        $.map(selection, function (item, key) {
            CHK_NO_A += item.get('CHK_NO_A') + ',';
            MMCODE_A += item.get('MMCODE_A') + ',';
        })
        Ext.Ajax.request({
            url: GetGoBack,
            method: reqVal_p,
            params: {
                CHK_NO_A: CHK_NO_A,
                MMCODE_A: MMCODE_A
            },
            //async: true,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('點選的品項已駁回');
                    T1LastRec = null;
                    T1Load();
                }
                else {
                    Ext.Msg.alert('失敗', '失敗');
                    msglabel("失敗");
                }
            },
            failure: function (form, action) {
                switch (action.failureType) {
                    case Ext.form.action.Action.CLIENT_INVALID:
                        Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                        msglabel(" Form fields may not be submitted with invalid values");
                        break;
                    case Ext.form.action.Action.CONNECT_FAILURE:
                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                        msglabel(" Ajax communication failed");
                        break;
                    case Ext.form.action.Action.SERVER_INVALID:
                        Ext.Msg.alert('失敗', action.result.msg);
                        msglabel(" " + action.result.msg);
                        break;
                }
            }
        })
    }

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
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }]
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    setCHK_TYPE();
    setCHKCLASS();
    T1Query.getForm().findField('P1').focus(); //讓游標停在P0這一格
    T1Query.getForm().findField('P1').setValue(new Date());
});
