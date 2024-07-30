﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    /**
     * Ext Override
     */
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    /**
     * General properties and methods
     */
    var T1GetExcel = '/api/CE0044/Excel';

    var baseUrl = function (actionUrl) {
        return '/api/CE0045/' + actionUrl;
    }
    var drawCellColor = function () {
        return function (val, meta, record) {
            return isBalance(val, meta, record)
                ? '<font color=red>' + val + '</font>'
                : val;
        }
    }

    /**
     *  T1 panel 
     */
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MMCODE', type: 'string' }, // 院內碼
            { name: 'MMNAME_C', type: 'string' }, // 中文品名
            { name: 'MMNAME_E', type: 'string' }, // 英文品名
            { name: 'BASE_UNIT', type: 'string' }, // 計量單位
            { name: 'INV_QTY', type: 'string' }, // 電腦量(庫存量)
            { name: 'CHK_QTY', type: 'string' }, // 盤點量
            { name: 'MEMO', type: 'string' }, //
            { name: 'OLD_INV_QTY', type: 'string' }, //上期結存
            { name: 'CURRENT_APPLY_AMOUNT', type: 'string' }, // 本月申請總量
            { name: 'ENDL', type: 'string' }
        ]
    });
    // 更新 狀態:  月結日期: 
    var fetchGrid = function () {
        Ext.Ajax.request({
            url: baseUrl('GetChkStatus'),
            params: {
                chk_ym: T1Query.getForm().findField('P0').getValue(), // 盤點年月 
                ur_inid: T1Query.getForm().findField('P1').getValue() // 責任中心
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success) {
                    var list = data.etts;
                    if (list.length > 0) {
                        var item = list[0];
                        T1Query.getForm().findField('CHK_STATUS_T').setValue(item.CHK_STATUS);
                        T1Query.getForm().findField('CHK_CLOSE_DATE_T').setValue(item.CHK_CLOSE_DATE);
                    }
                    else {
                        T1Query.getForm().findField('CHK_STATUS_T').setValue('');
                        T1Query.getForm().findField('CHK_CLOSE_DATE_T').setValue('');
                    }

                    T1Load();
                    T1Tool.moveFirst();
                }
            },
            failure: function (response, options) {

            }
        });
    }
    // 檢查必填欄位有沒有填值
    var canQuery = function () {
        return (
            (T1Query.getForm().findField('P0').getValue() == "" || T1Query.getForm().findField('P0').getValue() == null) ||
            (T1Query.getForm().findField('P1').getValue() == "" || T1Query.getForm().findField('P1').getValue() == null)
        ) === false;
    }
    //查詢月結年月
    var ymComboStore = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: baseUrl('GetYmCombo'),
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    //責任中心代碼
    var urInidComboStore = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: baseUrl('GetUrInidCombo'),
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 50, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }, { property: 'CHK_TIME', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: baseUrl('AllM'),
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }, listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    set_ym: T1Query.getForm().findField('P0').getValue(), // 盤點年月 
                    ur_inid: T1Query.getForm().findField('P1').getValue(), // 責任中心 
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, operation, eOpts) {
                console.log('資料已載入:', records);
            } // end of load
        }
    });

    // 上方查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        frame: false,
        layout: 'vbox',
        border: false,
        //autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 70,
            width: 230
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [{
                xtype: 'panel',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'combo',
                    store: ymComboStore,
                    fieldLabel: '盤點年月',
                    name: 'P0',
                    id: 'P0',
                    fieldCls: 'required',
                    allowBlank: false,
                    limit: 10, //限制一次最多顯示10筆
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    requiredFields: ['VALUE'],
                    width: 180,
                }, {
                    xtype: 'combo',
                    store: urInidComboStore,
                    fieldLabel: '責任中心<br>代碼',
                    name: 'P1',
                    id: 'P1',
                    queryMode: 'local',
                    labelAlign: 'right',
                    fieldCls: 'required',
                    allowBlank: false,
                    anyMatch: true,
                    autoSelect: true,
                    limit: 10, //限制一次最多顯示10筆
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    requiredFields: ['VALUE'],
                    editable: true,
                    typeAhead: true,
                    forceSelection: true,
                    selectOnFocus: true,
                    width: 180,
                }, {
                    xtype: 'button',
                    text: '查詢',
                    type: 'submit',
                    id: 'T1btn1',
                    handler: function () {
                        msglabel('訊息區:');

                        if (!canQuery()) {
                            Ext.Msg.alert('訊息', '需填盤點年月、責任中心代碼才能查詢');
                        }

                        msglabel('訊息區:');
                        fetchGrid();
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        T1Load();
                        Ext.getCmp('export').setDisabled(true);
                    }
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '狀態',
                    padding: '0 0 0 8',
                    labelAlign: 'right',
                    name: 'CHK_STATUS_T',
                    labelWidth: 120
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '月結日期',
                    padding: '0 0 0 8',
                    labelAlign: 'right',
                    name: 'CHK_CLOSE_DATE_T',
                    labelWidth: 120
                },
                ]
            }]
        }]
    });

    function T1Load() {
        Ext.getCmp('export').setDisabled(false);

        if ("盤中" === T1Query.getForm().findField('CHK_STATUS_T').getValue()) {
            Ext.getCmp('T1Import').setDisabled(false);
            Ext.getCmp('update').setDisabled(false);
        }
        else {
            Ext.getCmp('T1Import').setDisabled(true);
            Ext.getCmp('update').setDisabled(true);
        }
        T1Store.load({
            params: {
                start: 0
            }
        });
        T1Tool.moveFirst();
    }

    //表單工具列
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                xtype: 'filefield',
                name: 'T1Import',
                id: 'T1Import',
                buttonOnly: true,
                buttonText: '匯入',
                disabled: true,
                width: 40,
                listeners: {
                    change: function (widget, value, eOpts) {
                        var files = event.target.files;
                        var self = this; // the controller
                        if (!files || files.length == 0) return; // make sure we got something
                        var f = files[0];
                        var ext = this.value.split('.').pop();

                        if (!/^(xls|xlsx)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '請選擇xlsx或xls檔案！');
                            Ext.getCmp('import').fileInputEl.dom.value = '';
                            msglabel("請選擇xlsx或xls檔案！");
                        } else {
                            msglabel("已選擇檔案");
                            myMask.show();
                            var formData = new FormData();
                            formData.append("file", f);
                            formData.append("set_ym", T1Query.getForm().findField('P0').getValue());
                            formData.append("ur_inid", T1Query.getForm().findField('P1').getValue()); // 庫房代碼(供應中心)
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/CE0045/Import",
                                data: formData,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    console.log(data);
                                    if (!data.success) {
                                        Ext.MessageBox.alert("提示", data.msg);
                                        msglabel("訊息區:");
                                        Ext.getCmp('T1Import').setDisabled(true);
                                    }
                                    else {
                                        msglabel("訊息區:檔案匯入成功");
                                        Ext.getCmp('T1Import').fileInputEl.dom.value = '';
                                        T1Load();
                                    }
                                    myMask.hide();
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    Ext.getCmp('T1Import').setDisabled(true);
                                    myMask.hide();
                                }
                            }); // end of var ajaxRequest = $.ajax({
                        }
                    }
                }
            }, {
                id: 'export',
                itemId: 'export',
                text: '匯出',
                disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() }); //SQL篩選條件 
                            PostForm('../../api/CE0045/Excel', p);
                        }
                    });
                }
            }, {
                id: 'update',
                itemId: 'update',
                text: '儲存',
                disabled: true,
                handler: function () {
                    // handle 盤中狀態時才 enable
                    var tempData = T1Grid.getStore().data.items;
                    if (tempData.length === 0) {
                        return false;
                    }

                    var data = [];

                    for (var i = 0; i < tempData.length; i++) {
                        // WH_NO=前端T1Grid.MMCODE(院內碼)
                        if (tempData[i].dirty) {
                            data.push(tempData[i].data);
                        }
                    }

                    if (0 === data.length) {
                        return;
                    }

                    myMask.show();
                    // 
                    Ext.Ajax.request({
                        url: '../../../api/CE0045/UpdateDetails',
                        method: reqVal_p,
                        contentType: "application/json",
                        params: {
                            list: Ext.util.JSON.encode(data),
                            set_ym: T1Query.getForm().findField('P0').getValue(), // 盤點年月
                            ur_inid: T1Query.getForm().findField('P1').getValue(), // 責任中心 
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            myMask.hide();
                            if (data.success) {
                                var data = JSON.parse(response.responseText);
                                if (data.success == false) {
                                    Ext.Msg.alert('失敗', data.msg);
                                    return;
                                }
                                msglabel('訊息區:資料更新成功');
                                T1Load();
                            } else {
                                Ext.Msg.alert('失敗', 'Ajax communication failed');
                            }
                        },

                        failure: function (response, action) {
                            myMask.hide();
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                        }
                    });
                }
            }, {

                xtype: 'displayfield',
                fieldLabel: '',
                value: '<span style="color:red">手動輸入盤點量請一頁儲存一次</span>',
                padding: '0 0 0 20'
            }
        ]
    });
    // 查詢結果資料列表(上半部)
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            { xtype: 'rownumberer' },
            { text: "院內碼", dataIndex: "MMCODE", locked: true, width: 85 },
            { text: "中文品名", dataIndex: "MMNAME_C", locked: true, width: 120 },
            { text: "英文品名", dataIndex: "MMNAME_E", locked: true, width: 120 },
            { text: "計量單位", dataIndex: "BASE_UNIT", width: 85 },
            { text: "電腦量", dataIndex: "INV_QTY", width: 85 },
            {
                text: "盤點量",
                style: 'text-align:left; color:red',
                dataIndex: "CHK_QTY",
                editor: {
                    xtype: 'textfield',
                    regexText: '只能輸入數字',
                    regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                    selectOnFocus: true
                },
                width: 85
            },
            { text: "盤點人員", dataIndex: "CHK_UID", width: 85 },
            { text: "盤點時間", dataIndex: "CHK_TIME", width: 85 },
            {
                text: "備註",
                dataIndex: "MEMO",
                style: 'text-align:left; color:red',
                width: 140,
                editor: {
                    xtype: 'textfield',
                    selectOnFocus: true,
                },
            },
            { header: "", flex: 1 }
        ],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1
            })
        ],
        listeners: {
            beforeedit: function (e, eOpts) {
                // CHK_QTY 盤點量，在非盤中時不得編輯
                if ("CHK_QTY" === eOpts.field &&
                    "盤中" !== T1Query.getForm().findField('CHK_STATUS_T').getValue()
                ) {
                    return false;
                }
            }
        },
    });

    // View port view
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
            layout: {
                type: 'border'
            },
            collapsible: false,
            title: '',
            items: [
                {
                    region: 'center',
                    layout: 'fit',
                    collapsible: false,
                    split: true,
                    border: false,
                    items: [T1Grid]
                }
            ]
        }]
    });

    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    myMask.hide();
});