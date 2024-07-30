Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);


Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    //#region 參數
    // ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ 參數 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        // var T1Get = '../../../api/AB0053/All_1';
        var T1Name = '各庫效期表';
        var T2Name = '效期總表';
        var T3Name = '藥庫專區';

        var T1Rec = 0;
        var T1LastRec = null;
        var T2Rec = 0;
        var T2LastRec = null;
        var T3Rec = 0;
        var T3LastRec = null;

        var WhnoGet = '../../../api/AB0053/GetWhno';

        var T1Set = '';
        var T3Set = '';

        var T1GetExcel = '/api/AB0053/Excel_T1';
        var T2GetExcel = '/api/AB0053/Excel_T2';
        var T3GetExcel = '/api/AB0053/Excel_T3';

        var windowHeight = $(window).height();
        var windowWidth = $(window).width();
    }
    //#endregion

    //#region T1
    // ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ T1 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        // 庫房別清單(搜尋)
        var WhnoQueryStore = Ext.create('Ext.data.Store', {
            fields: ['VALUE', 'TEXT']
        });

        // 建立庫房別清單(搜尋)
        function setDefaultWhno() {
            WhnoQueryStore.add({ VALUE: '', TEXT: '全部' });
            WhnoQueryStore.add({ VALUE: 'PH1S', TEXT: 'PH1S 藥庫' });
            WhnoQueryStore.add({ VALUE: 'PH1A', TEXT: 'PH1A 內湖住院藥局' });
            WhnoQueryStore.add({ VALUE: 'PH1C', TEXT: 'PH1C 內湖門診藥局' });
            WhnoQueryStore.add({ VALUE: 'PH1R', TEXT: 'PH1R 內湖急診藥局' });
            WhnoQueryStore.add({ VALUE: 'PHMC', TEXT: 'PHMC 汀州藥局' });
            WhnoQueryStore.add({ VALUE: 'CHEMO', TEXT: 'CHEMO 內湖化療調配室' });
            WhnoQueryStore.add({ VALUE: 'CHEMOT', TEXT: 'CHEMOT 汀州化療調配室' });
            WhnoQueryStore.add({ VALUE: 'TPN', TEXT: 'TPN 製劑室' });
            T1Query.getForm().findField('P0').setValue('');
            /* Ext.Ajax.request({
                 url: WhnoGet,
                 method: reqVal_g,
                 success: function (response) {
                     var data = Ext.decode(response.responseText);
                     if (data.success) {
                         var whnos = data.etts;
                         if (whnos.length > 0) {
                             WhnoQueryStore.add({ VALUE: '', TEXT: '全部' });
                             for (var i = 0; i < whnos.length; i++) {
                                 WhnoQueryStore.add({ VALUE: whnos[i].VALUE, TEXT: whnos[i].TEXT });
                             }
                             T1Query.getForm().findField("P0").setValue("");
                         }
                     }
                 },
                 failure: function (response, options) {
 
                 }
             });*/
        }

        // T1 QUERY
        var T1Query = Ext.widget({
            xtype: 'form',
            layout: 'form',
            border: false,
            autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
            fieldDefaults: {
                xtype: 'textfield',
                labelAlign: 'right',
                labelWidth: 50,
                width: 210
            },
            items: [{
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '庫房別',
                        store: WhnoQueryStore,
                        name: 'P0',
                        id: 'P0',
                        // enforceMaxLength: true, // 限制可輸入最大長度
                        // maxLength: 100, // 可輸入最大長度為100
                        padding: '0 4 0 4',
                        //labelWidth: 50,
                        //width: 210,
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        autoSelect: true,
                        anyMatch: true,
                        multiSelect: false,
                        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                        allowBlank: true, // 欄位是否為必填
                        blankText: "請選擇庫房別"
                    }, {
                        xtype: 'monthfield',
                        fieldLabel: '月份',
                        name: 'P1',
                        id: 'P1',
                        enforceMaxLength: true,
                        padding: '0 4 0 4',
                        allowBlank: true, // 欄位是否為必填
                        format: 'Xm',
                        value: new Date()
                    }, {
                        xtype: 'button',
                        text: '查詢',
                        handler: function () {
                            msglabel("");
                            T1Load();
                            viewport.down('#form').setCollapsed("true");

                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            msglabel("");
                            f.reset();
                            f.findField('P0').setValue('');
                            f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        }
                    }
                ]
            }]
        });

        // T1 Store
        var T1Store = Ext.create('WEBAPP.store.AB0053_1', { // 定義於/Scripts/app/store/AB0053_1.js
            listeners: {
                beforeload: function (store, options) {
                    // 載入前將查詢條件P0~P2的值代入參數
                    var np = {
                        p0: T1Query.getForm().findField('P0').getValue(),
                        p1: T1Query.getForm().findField('P1').rawValue
                    };
                    Ext.apply(store.proxy.extraParams, np);
                }
            }
        });

        // T1 TOOLBAR,包含換頁、新增/修改/刪除鈕
        var T1Tool = Ext.create('Ext.PagingToolbar', {
            store: T1Store,
            displayInfo: true,
            border: false,
            plain: true,
            buttons: [
                {
                    itemId: 'export1', text: '匯出', handler: function () {
                        var p = new Array();
                        p.push({ name: 'FN', value: '各庫效期表.xls' });
                        p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                        p.push({ name: 'p1', value: T1Query.getForm().findField('P1').rawValue });
                        PostForm(T1GetExcel, p);
                        msglabel('匯出完成');
                    }
                },
                {
                    itemId: 'return_close', text: '回報截止', handler: function () {
                        T1Set = '/api/AB0053/Return_Close';
                        T1Submit();
                    }
                }
            ]
        });

        // T1 GRID查詢結果
        var T1Grid = Ext.create('Ext.grid.Panel', {
            //title: T1Name,
            store: T1Store,
            plain: true,
            loadingText: '處理中...',
            loadMask: true,
            cls: 'T1',
            dockedItems: [{
                /*dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',*/
                items: [T1Query]
            }, {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }],
            columns: [{
                xtype: 'rownumberer',
                width: 30,
                align: 'Center',
                labelAlign: 'Center'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 200
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 250
            }, {
                text: "儲位碼",
                dataIndex: 'STORE_LOC',
                width: 100
            }, {
                text: "藥品批號",
                dataIndex: 'LOT_NO',
                width: 110
            }, {
                text: "回覆效期",
                dataIndex: 'REPLY_DATE',
                width: 80
            }, {
                text: "效期藥量",
                dataIndex: 'EXP_QTY',
                width: 70,
                align: 'right',
                style: 'text-align:left',
                xtype: 'numbercolumn',
                format: '0'
            }, {
                text: "備註",
                dataIndex: 'MEMO',
                width: 100
            }, {
                text: "回覆日期",
                dataIndex: 'REPLY_TIME',
                width: 80
            }, {
                text: "回覆人員",
                dataIndex: 'REPLY_ID',
                width: 80
            }, {
                text: "結案日期",
                dataIndex: 'CLOSE_TIME',
                width: 80
            }, {
                text: "截止人員",
                dataIndex: 'CLOSE_ID',
                width: 80
            }, {
                text: "月份",
                dataIndex: 'EXP_DATE',
                width: 60
            }, {
                text: "庫別代碼",
                dataIndex: 'WH_NO',
                width: 80
            }, {
                text: "庫別名稱",
                dataIndex: 'WH_NAME',
                width: 100
            }, {
                text: "效期回覆狀態",
                dataIndex: 'EXP_STAT_NAME',
                width: 85
            }],
            listeners: {
                click: {
                    element: 'el',
                    fn: function () {
                        T1Form.setVisible(true);
                        T2Form.setVisible(false);
                        T3Form.setVisible(false);
                    }
                },
                selectionchange: function (model, records) {
                    T1Rec = records.length;
                    T1LastRec = records[0];
                    viewport.down('#form').expand();
                    setFormT1a();
                    if (T1LastRec) {
                        msglabel("");
                    }
                }
            }
        });

        // T1 顯示明細/新增/修改輸入欄
        var T1Form = Ext.widget({
            xtype: 'form',
            layout: 'form',
            frame: false,
            cls: 'T1b',
            title: '',
            autoScroll: true,
            bodyPadding: '10',
            fieldDefaults: {
                labelAlign: 'right',
                msgTarget: 'side',
                labelWidth: 90
            },
            defaultType: 'textfield',
            items: [{
                xtype: 'displayfield',
                fieldLabel: '院內碼', //白底顯示
                name: 'MMCODE',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '中文品名', //白底顯示
                name: 'MMNAME_C',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '英文品名', //白底顯示
                name: 'MMNAME_E',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '儲位碼', //白底顯示
                name: 'STORE_LOC',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '藥品批號', //白底顯示
                name: 'LOT_NO',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '回覆效期', //白底顯示
                name: 'REPLY_DATE',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '效期藥量', //白底顯示
                name: 'EXP_QTY',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '備註', //白底顯示
                name: 'MEMO',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '回覆日期', //白底顯示
                name: 'REPLY_TIME',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '回覆人員', //白底顯示
                name: 'REPLY_ID',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '結案日期', //白底顯示
                name: 'CLOSE_TIME',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '截止人員', //白底顯示
                name: 'CLOSE_ID',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '月份', //白底顯示
                name: 'EXP_DATE',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '庫別代碼', //白底顯示
                name: 'WH_NO',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '庫別名稱', //白底顯示
                name: 'WH_NAME',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '效期回覆狀態', //白底顯示
                name: 'EXP_STAT_NAME',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }]
        });

        // T1 點擊資料列
        function setFormT1a() {
            if (T1LastRec) {
                isNew = false;
                T1Form.loadRecord(T1LastRec);
                var f = T1Form.getForm();
                var u = f.findField('MMCODE');
                u.setReadOnly(true);
                u.setFieldStyle('border: 0px');
            }
            else {
                T1Form.getForm().reset();
            }
        }

        // T1 按下回報截止
        function T1Submit() {
            ///var deferred = new Ext.Deferred();
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            Ext.Ajax.request({
                url: T1Set,
                method: reqVal_p,
                success: function (response) {
                    //deferred.resolve(response.responseText);
                    myMask.hide();
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        T1Load();
                        msglabel(" 回報截止成功");
                        Ext.Msg.alert('提醒', '回報截止成功');
                        T1LastRec = false;
                        T1Form.getForm().reset();
                    }
                    else {
                        msglabel(" 回報截止發生錯誤: " + data.msg);
                        Ext.Msg.alert('提醒', " 回報截止發生錯誤: " + data.msg);
                    }
                },

                failure: function (response) {
                    myMask.hide();
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
            });
        }
    }
    
//#endregion

    //#region T2
    // ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ T2 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        // T2 QUERY
        var T2Query = Ext.widget({
            xtype: 'form',
            layout: 'form',
            border: false,
            autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
            fieldDefaults: {
                xtype: 'textfield',
                labelAlign: 'right',
                labelWidth: 50,
                width: 210
            },
            items: [{
                xtype: 'panel',
                id: 'PanelP2',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'monthfield',
                    fieldLabel: '月份',
                    name: 'P2',
                    id: 'P2',
                    enforceMaxLength: true,
                    padding: '0 4 0 4',
                    allowBlank: true, // 欄位是否為必填
                    format: 'Xm',
                    value: new Date()
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        msglabel("");
                        T2Load();
                        viewport.down('#form').setCollapsed("true");
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        msglabel("");
                        f.reset();
                        f.findField('P2').setValue('');
                        f.findField('P2').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
                ]
            }]
        });

        // T2 STORE
        var T2Store = Ext.create('WEBAPP.store.AB0053_2', { // 定義於/Scripts/app/store/AB0053_2.js
            listeners: {
                beforeload: function (store, options) {
                    // 載入前將查詢條件P0的值代入參數
                    var np = {
                        p2: T2Query.getForm().findField('P2').rawValue
                    };
                    Ext.apply(store.proxy.extraParams, np);
                }
            }
        });

        // T2 TOOLBAR,包含換頁、新增/修改/刪除鈕
        var T2Tool = Ext.create('Ext.PagingToolbar', {
            store: T2Store,
            displayInfo: true,
            border: false,
            plain: true,
            buttons: [
                {
                    itemId: 'export2', text: '匯出', handler: function () {
                        var p = new Array();
                        p.push({ name: 'FN', value: '效期總表.xls' });
                        p.push({ name: 'p2', value: T2Query.getForm().findField('P2').rawValue });
                        PostForm(T2GetExcel, p);
                        msglabel('匯出完成');
                    }
                }
            ]
        });

        // T2 GRID查詢結果
        var T2Grid = Ext.create('Ext.grid.Panel', {
            //title: T2Name,
            store: T2Store,
            plain: true,
            loadingText: '處理中...',
            loadMask: true,
            cls: 'T1',
            dockedItems: [{
                /*dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',*/
                items: [T2Query]
            }, {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }],
            columns: [{
                xtype: 'rownumberer',
                width: 30,
                align: 'Center',
                labelAlign: 'Center'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 200
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 250
            }, {
                text: "月份",
                dataIndex: 'EXP_DATE',
                width: 60
            }, {
                text: "藥品批號",
                dataIndex: 'LOT_NO',
                width: 110
            }, {
                text: "回覆效期",
                dataIndex: 'REPLY_DATE',
                width: 80
            }, {
                text: "藥庫(PH1S)",
                dataIndex: 'PH1S',
                width: 150,
                align: 'right',
                style: 'text-align:left',
                xtype: 'numbercolumn',
                format: '0'
            }, {
                text: "內湖化療調配室(CHEMO)",
                dataIndex: 'CHEMO',
                width: 150,
                align: 'right',
                style: 'text-align:left',
                xtype: 'numbercolumn',
                format: '0'
            }, {
                text: "汀洲化療調配室(CHEMOT)",
                dataIndex: 'CHEMOT',
                width: 150,
                align: 'right',
                style: 'text-align:left',
                xtype: 'numbercolumn',
                format: '0'
            }, {
                text: "內湖住院藥局(PH1A)",
                dataIndex: 'PH1A',
                width: 150,
                align: 'right',
                style: 'text-align:left',
                xtype: 'numbercolumn',
                format: '0'
            }, {
                text: "內湖門診藥局(PH1C)",
                dataIndex: 'PH1C',
                width: 150,
                align: 'right',
                style: 'text-align:left',
                xtype: 'numbercolumn',
                format: '0'
            }, {
                text: "內湖急診藥局(PH1R)",
                dataIndex: 'PH1R',
                width: 150,
                align: 'right',
                style: 'text-align:left',
                xtype: 'numbercolumn',
                format: '0'
            }, {
                text: "汀洲藥局(PHMC)",
                dataIndex: 'PHMC',
                width: 150,
                align: 'right',
                style: 'text-align:left',
                xtype: 'numbercolumn',
                format: '0'
            }, {
                text: "製劑室(TPN)",
                dataIndex: 'TPN',
                width: 150,
                align: 'right',
                style: 'text-align:left',
                xtype: 'numbercolumn',
                format: '0'
            }, {
                text: "劑量單位",
                dataIndex: 'BASE_UNIT',
                width: 70
            }, {
                text: "廠商",
                dataIndex: 'E_MANUFACT',
                width: 120
            }, {
                text: "總數",
                dataIndex: 'EXP_QTY',
                width: 70,
                align: 'right',
                style: 'text-align:left',
                xtype: 'numbercolumn',
                format: '0'
            }],
            listeners: {
                click: {
                    element: 'el',
                    fn: function () {
                        T1Form.setVisible(false);
                        T2Form.setVisible(true);
                        T3Form.setVisible(false);
                    }
                },
                selectionchange: function (model, records) {
                    T2Rec = records.length;
                    T2LastRec = records[0];
                    viewport.down('#form').expand();
                    setFormT2a();
                    if (T2LastRec) {
                        msglabel("");
                    }
                }
            }
        });

        // 顯示明細/新增/修改輸入欄
        var T2Form = Ext.widget({
            xtype: 'form',
            layout: 'form',
            frame: false,
            cls: 'T1b',
            title: '',
            autoScroll: true,
            bodyPadding: '10',
            fieldDefaults: {
                labelAlign: 'right',
                msgTarget: 'side',
                labelWidth: 90
            },
            defaultType: 'textfield',
            items: [{
                xtype: 'displayfield',
                fieldLabel: '院內碼', //白底顯示
                name: 'MMCODE',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '中文品名', //白底顯示
                name: 'MMNAME_C',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '英文品名', //白底顯示
                name: 'MMNAME_E',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '月份', //白底顯示
                name: 'EXP_DATE',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '藥品批號', //白底顯示
                name: 'LOT_NO',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '回覆效期', //白底顯示
                name: 'REPLY_DATE',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '藥庫(PH1S)', //白底顯示
                name: 'PH1S',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '內湖化療調配室(CHEMO)', //白底顯示
                name: 'CHEMO',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '汀洲化療調配室(CHEMOT)', //白底顯示
                name: 'CHEMOT',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '內湖住院藥局(PH1A)', //白底顯示
                name: 'PH1A',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '內湖門診藥局(PH1C)', //白底顯示
                name: 'PH1C',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '內湖急診藥局(PH1R)', //白底顯示
                name: 'PH1R',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '汀洲藥局(PHMC)', //白底顯示
                name: 'PHMC',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '製劑室(TPN)', //白底顯示
                name: 'TPN',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '劑量單位', //白底顯示
                name: 'BASE_UNIT',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '廠商', //白底顯示
                name: 'E_MANUFACT',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '總數', //白底顯示
                name: 'EXP_QTY',
                enforceMaxLength: true,
                maxLength: 80,
                readOnly: true
            }]
        });

        // T2 點擊資料列
        function setFormT2a() {
            if (T2LastRec) {
                isNew = false;
                T2Form.loadRecord(T2LastRec);
                var f = T2Form.getForm();
                var u = f.findField('MMCODE');
                u.setReadOnly(true);
                u.setFieldStyle('border: 0px');
            }
            else {
                T2Form.getForm().reset();
            }
        }
    }
//#endregion

    //#region T3
    // ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ T3 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        //結案狀態清單
        var CloseFlagStore = Ext.create('Ext.data.Store', {
            fields: ['VALUE', 'TEXT']
        });

        //建立結案狀態清單
        function setCloseFlagCombo() {
            CloseFlagStore.add({ VALUE: ' ', TEXT: '全部' });
            CloseFlagStore.add({ VALUE: 'Y', TEXT: 'Y 是' });
            CloseFlagStore.add({ VALUE: 'N', TEXT: 'N 否' });
        }

        //寄送狀態清單 query
        var MailStatusQueryStore = Ext.create('Ext.data.Store', {
            fields: ['VALUE', 'TEXT']
        });

        //寄送狀態清單 query
        function setMailStatusQueryCombo() {
            MailStatusQueryStore.add({ VALUE: ' ', TEXT: '全部' });
            MailStatusQueryStore.add({ VALUE: '1', TEXT: '未寄送' });
            MailStatusQueryStore.add({ VALUE: '2', TEXT: '未回覆' });
            MailStatusQueryStore.add({ VALUE: '3', TEXT: '已回覆' });
        }


        //搜尋院內碼
        var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
            name: 'P3',
            id: 'P3',
            fieldLabel: '院內碼',
            allowBlank: true,
            labelWidth: 50,
            width: 210,
            padding: '0 0 0 4',

            //限制一次最多顯示10筆
            limit: 10,

            //指定查詢的Controller路徑
            queryUrl: '/api/AB0053/GetMMCodeCombo',

            //查詢完會回傳的欄位
            extraFields: ['MAT_CLASS', 'BASE_UNIT']
        });

        //新增院內碼
        var mmCodeCombo_i = Ext.create('WEBAPP.form.MMCodeCombo', {
            name: 'MMCODE',
            fieldLabel: '院內碼',
            fieldCls: 'required',
            allowBlank: false,
            //labelWidth: 110,
            labelAlign: 'right',
            //width: 250,
            blankText: "請輸入院內碼",

            //限制一次最多顯示10筆
            limit: 10,

            //指定查詢的Controller路徑
            queryUrl: '/api/AB0053/GetMMCodeCombo',

            //查詢完會回傳的欄位
            //extraFields: ['MAT_CLASS', 'BASE_UNIT'],
            listeners: {
                select: function (c, r, i, e) {
                    //選取下拉項目時，顯示回傳值
                    //alert(r.get('MAT_CLASS'));
                    var f = T3Form.getForm();
                    if (r.get('MMCODE') !== '') {
                        f.findField("MMCODE").setValue(r.get('MMCODE'));
                        f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
                        f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                        f.findField("AGEN_NAMEC").setValue(r.get('AGEN_NAMEC'));
                    }
                }
            }
        });

        //廠商下拉選單
        var agennoCombo = Ext.create('WEBAPP.form.AgenNoCombo', {
            name: 'P7',
            fieldLabel: '廠商代碼',
            allowBlank: true,
            labelWidth: 70,
            labelAlign: 'right',
            width: 190,
            //限制一次最多顯示10筆
            limit: 10,

            //指定查詢的Controller路徑
            queryUrl: '/api/AB0053/GetAgennoCombo',

            //查詢完會回傳的欄位
            //extraFields: ['MAT_CLASS', 'BASE_UNIT'],
            //listeners: {
            //    select: function (c, r, i, e) {
            //        //選取下拉項目時，顯示回傳值
            //        //alert(r.get('MAT_CLASS'));
            //        var f = T3Query.getForm();
            //        if (r.get('MMCODE') !== '') {
            //            f.findField("MMCODE").setValue(r.get('MMCODE'));
            //            f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
            //            f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
            //            f.findField("AGEN_NAMEC").setValue(r.get('AGEN_NAMEC'));
            //        }
            //    }
            //}
        });

        function setMmcode(args) {
            if (args.MMCODE !== '') {
                T3Query.getForm().findField("P3").setValue(args.MMCODE);
            }
        }

        function setMmcode_i(args) {
            if (args.MMCODE !== '') {
                T3Form.getForm().findField("MMCODE").setValue(args.MMCODE);
                T3Form.getForm().findField("MMNAME_C").setValue(args.MMNAME_C);
                T3Form.getForm().findField("MMNAME_E").setValue(args.MMNAME_E);
                T3Form.getForm().findField("AGEN_NAMEC").setValue(args.AGEN_NAMEC);
            }
        }

        //  T3 QUERY
        var T3Query = Ext.widget({
            xtype: 'form',
            layout: 'form',
            border: false,
            autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
            fieldDefaults: {
                xtype: 'textfield',
                labelAlign: 'right',
                labelWidth: 50,
                width: 210
            },
            items: [{
                xtype: 'panel',
                id: 'PanelP3',
                border: false,
                layout: 'hbox',
                items: [
                    mmCodeCombo, {
                        xtype: 'button',
                        itemId: 'btnMmcode',
                        iconCls: 'TRASearch',
                        handler: function () {
                            var f = T3Query.getForm();
                            popMmcodeForm(viewport, '/api/AB0053/GetMmcode', { MMCODE: f.findField("P3").getValue() }, setMmcode);
                        }
                    }, {
                        xtype: 'monthfield',
                        fieldLabel: '月份',
                        name: 'P4',
                        id: 'P4',
                        enforceMaxLength: true,
                        padding: '0 4 0 4',
                        allowBlank: true, // 欄位是否為必填
                        format: 'Xm',
                        value: new Date()
                    },
                    {
                        xtype: 'combo',
                        store: CloseFlagStore,
                        fieldLabel: '結案否',
                        name: 'P5',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        anyMatch: true,
                        fieldCls:'required',
                        autoSelect: true,
                        multiSelect: false,
                        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                        matchFieldWidth: true,
                        allowBlank: false,
                        width: 135,
                        value:' ',
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    },
                    {
                        xtype: 'combo',
                        store: MailStatusQueryStore,
                        fieldLabel: 'Mail狀態',
                        name: 'P6',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        fieldCls: 'required',
                        anyMatch: true,
                        autoSelect: true,
                        multiSelect: false,
                        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                        matchFieldWidth: true,
                        allowBlank: false,
                        width: 150,
                        value: ' ',
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    },
                    agennoCombo, {
                        xtype: 'checkboxfield',
                        boxLabel: '回報數量 > 0',
                        name: 'P8',
                        id: 'P8',
                        style: 'margin:0px 5px 0px 5px;',
                        labelWidth: 80,
                        width: 90,
                        value: true
                    }, {
                        xtype: 'checkboxfield',
                        boxLabel: '上月未完成',
                        name: 'P9',
                        id: 'P9',
                        style: 'margin:0px 5px 0px 5px;',
                        labelWidth: 80,
                        width: 90,
                    },
                    {
                        xtype: 'button',
                        text: '查詢',
                        handler: function () {
                            msglabel("");
                            T3Load(true);
                            viewport.down('#form').setCollapsed("true");
                            T3Grid.down('#edit').setDisabled(true);
                            T3Grid.down('#delete').setDisabled(true);
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            msglabel("");
                            f.findField('P3').setValue('');
                            f.findField('P4').setValue('');
                            f.findField('P7').setValue('');
                            f.findField('P3').focus(); // 進入畫面時輸入游標預設在P0


                            f.findField('P5').setValue(defaultCloseFlagArray);
                            f.findField('P6').setValue(defaultMailStatusArray);
                        }
                    }
                ]
            }]
        });

        // T3 STORE
        var T3Store = Ext.create('WEBAPP.store.AB0053_3', { // 定義於/Scripts/app/store/AB0053_3.js
            listeners: {
                beforeload: function (store, options) {
                    // 載入前將查詢條件P0~P2的值代入參數
                    var np = {
                        p3: T3Query.getForm().findField('P3').getValue(),
                        p4: T3Query.getForm().findField('P4').rawValue,
                        p5: T3Query.getForm().findField('P5').getValue(),
                        p6: T3Query.getForm().findField('P6').getValue(),
                        p7: T3Query.getForm().findField('P7').getValue(),
                        p8: T3Query.getForm().findField('P8').checked ? "Y" : "N",
                        p9: T3Query.getForm().findField('P9').checked ? "Y" : "N",
                    };
                    Ext.apply(store.proxy.extraParams, np);
                }
            }
        });

        // T3 TOOLBAR,包含換頁、新增/修改/刪除鈕
        var T3Tool = Ext.create('Ext.PagingToolbar', {
            store: T3Store,
            displayInfo: true,
            border: false,
            plain: true,
            buttons: [{
                text: '新增', handler: function () {
                    T3Set = '/api/AB0053/Create'; // AB0053Controller的Create
                    setFormT3('I', '新增');
                }
            }, {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T3Set = '/api/AB0053/Update';
                    setFormT3("U", '修改');
                }
            }, {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    msglabel("");
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T3Set = '/api/AB0053/Delete';
                            T3Form.getForm().findField('x').setValue('D');
                            T3Submit();
                        }
                    });
                }
            }, {
                itemId: 'export3', text: '匯出', handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '藥庫專區.xls' });
                    p.push({ name: 'p3', value: T3Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T3Query.getForm().findField('P4').rawValue });
                    p.push({ name: 'p5', value: T3Query.getForm().findField('P5').getValue() });
                    p.push({ name: 'p6', value: T3Query.getForm().findField('P6').getValue() });
                    PostForm(T3GetExcel, p);
                    msglabel('匯出完成');
                }
            }, {
                    itemId: 'sendemail', text: '發送EMAIL',id:'sendemail', disabled: true, handler: function () {

                        var selection = T3Grid.getSelection();
                        var list = [];
                        for (var i = 0; i < selection.length; i++) {
                            var item = selection[i].data;
                            list.push(item);
                        }

                        if (list.length == 0) {
                            Ext.Msg.alert('提醒', '請選取要發送EMAIL的資料');
                            return;
                        }

                    Ext.MessageBox.confirm('發送EMAIL', '是否確定發送EMAIL？', function (btn, text) {
                        if (btn === 'yes') {
                            T3Set = '/api/AB0053/SendMail';

                            SendMail_v2(list);
                        }
                    });
                }
            }, {
                itemId: 'copydata', text: '匯入上月未完成品項', disabled: false, handler: function () {
                    Ext.MessageBox.confirm('匯入上月未完成品項', '是否匯入過往未結案資料至本月？', function (btn, text) {
                        if (btn === 'yes') {
                            T3Set = '/api/AB0053/CopyData';
                            CopyData();
                        }
                    });
                }
            },
                '<span style="color:red">(廠商有修改過為紅色)</span>'
            ]
        });

        // T3 GRID查詢結果
        var T3Grid = Ext.create('Ext.grid.Panel', {
            //title: T1Name,
            store: T3Store,
            plain: true,
            loadingText: '處理中...',
            loadMask: true,
            cls: 'T1',
            selType: 'checkboxmodel',
            dockedItems: [{
                /*dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',*/
                items: [T3Query]
            }, {
                dock: 'top',
                xtype: 'toolbar',
                items: [T3Tool]
            }],
            columns: [{
                xtype: 'rownumberer',
                width: 30,
                align: 'Center',
                labelAlign: 'Center'
            }, {
                dataIndex: 'MMCODE_DISPLAY',
                text: "院內碼",
                hidden: true
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80,
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.IS_AGENNO == 'N') {
                        return '<span>' + value + '</span>';
                    }
                    return '<span style="color:red">' + value + '</span>';
                },
            }, {
                text: "廠商名稱",
                dataIndex: 'comb_AGEN',
                width: 200,
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.IS_AGENNO == 'N') {
                        return '<span>' + value + '</span>';
                    }
                    return '<span style="color:red">' + value + '</span>';
                },
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 200,
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.IS_AGENNO == 'N') {
                        return '<span>' + value + '</span>';
                    }
                    return '<span style="color:red">' + value + '</span>';
                },
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 250,
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.IS_AGENNO == 'N') {
                        return '<span>' + value + '</span>';
                    }
                    return '<span style="color:red">' + value + '</span>';
                },
            }, {
                text: "月份",
                dataIndex: 'EXP_DATE_DISPLAY',
                width: 60,
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.IS_AGENNO == 'N') {
                        return '<span>' + value + '</span>';
                    }
                    return '<span style="color:red">' + value + '</span>';
                },
            }, {
                text: "藥品批號",
                dataIndex: 'LOT_NO',
                width: 110,
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.IS_AGENNO == 'N') {
                        return '<span>' + value + '</span>';
                    }
                    return '<span style="color:red">' + value + '</span>';
                },
            }, {
                text: "警示效期(年/月)",
                dataIndex: 'WARNYM',
                width: 100,
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.IS_AGENNO == 'N') {
                        return '<span>' + value + '</span>';
                    }
                    return '<span style="color:red">' + value + '</span>';
                },
            }, {
                text: "數量",
                dataIndex: 'EXP_QTY',
                width: 70,
                align: 'right',
                style: 'text-align:left',
                xtype: 'numbercolumn',
                format: '0',
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.IS_AGENNO == 'N') {
                        return '<span>' + value + '</span>';
                    }
                    return '<span style="color:red">' + value + '</span>';
                },
            }, {
                text: "各單位回報量",
                dataIndex: 'WH_EXPQTY',
                width: 100
            }, {
                text: "備註",
                dataIndex: 'MEMO',
                width: 150,
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.IS_AGENNO == 'N') {
                        return '<span>' + value + '</span>';
                    }
                    return '<span style="color:red">' + value + '</span>';
                },
            }, {
                text: "結案否",
                dataIndex: 'CLOSEFLAG_NAME',
                width: 60,
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.IS_AGENNO == 'N') {
                        return '<span>' + value + '</span>';
                    }
                    return '<span style="color:red">' + value + '</span>';
                },
            }, {
                text: "MAIL狀態",
                dataIndex: 'MAIL_STATUS',
                width: 70,
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.IS_AGENNO == 'N') {
                        return '<span>' + value + '</span>';
                    }
                    return '<span style="color:red">' + value + '</span>';
                },
            }, {
                text: "廠商EMAIL",
                dataIndex: 'EMAIL',
                width: 150,
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.IS_AGENNO == 'N') {
                        return '<span>' + value + '</span>';
                    }
                    return '<span style="color:red">' + value + '</span>';
                },
            }],
            listeners: {
                click: {
                    element: 'el',
                    fn: function () {
                        T1Form.setVisible(false);
                        T2Form.setVisible(false);
                        T3Form.setVisible(true);
                    }
                },
                itemclick: function (self, record, item, index, e, eOpts) {
                    T3Rec = index;
                    T3LastRec = record;
                    viewport.down('#form').expand();
                    setFormT3a();
                    if (T3LastRec) {
                        msglabel("");
                    }
                },
                selectionchange: function (model, records) {
                    //
                    //msglabel('');

                    //if (T1cell == '')
                    //
                    Ext.getCmp('sendemail').disable();

                    if (records.length > 0) {
                        Ext.getCmp('sendemail').enable();
                    }
                }
            }
        });

        // T3 顯示明細/新增/修改輸入欄
        var T3Form = Ext.widget({
            xtype: 'form',
            layout: 'form',
            frame: false,
            cls: 'T1b',
            title: '',
            autoScroll: true,
            bodyPadding: '10 0 0',
            fieldDefaults: {
                labelAlign: 'right',
                msgTarget: 'side',
                labelWidth: 100,
                //width: 280
            },
            items: [
                {
                    xtype: 'container',
                    style: 'border-spacing:0',
                    defaultType: 'textfield',
                    items: [
                        {
                            fieldLabel: 'Update',
                            name: 'x',
                            xtype: 'hidden'
                        },
                        {
                            fieldLabel: 'WARNYM_KEY',
                            name: 'WARNYM_KEY',
                            xtype: 'hidden',
                            submitValue: true
                        },
                        {
                            xtype: 'container',
                            layout: 'hbox',
                            //padding: '0 0 7 0',
                            id: 'mmcodeComboSet_i',
                            items: [
                                mmCodeCombo_i,
                                {
                                    xtype: 'button',
                                    itemId: 'btnMmcode_i',
                                    iconCls: 'TRASearch',
                                    handler: function () {
                                        var f = T3Form.getForm();
                                        popMmcodeForm(viewport, '/api/AB0053/GetMmcode', { MMCODE: f.findField("MMCODE").getValue() }, setMmcode_i);
                                    }
                                }
                            ]
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '院內碼', //白底顯示
                            name: 'MMCODE_DISPLAY',
                            readOnly: true
                        },
                        {
                            xtype: 'container',
                            layout: 'hbox',
                            //padding: '0 0 7 0',
                            id: 'agen_name',
                            items: [
                                {
                                    xtype: 'displayfield',
                                    fieldLabel: '廠商名稱', //白底顯示
                                    name: 'AGEN_NAMEC',
                                    enforceMaxLength: true,
                                    maxLength: 80
                                },
                                {
                                    xtype: 'button',
                                    text: '修改廠商',
                                    id:'changeVender',
                                    hidden: true,
                                    handler: function(){
                                        venderWindow.show();
                                    }
                                }
                            ]
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '中文品名', //白底顯示
                            name: 'MMNAME_C',
                            enforceMaxLength: true,
                            maxLength: 80,
                            readOnly: true
                        }, {
                            xtype: 'displayfield',
                            fieldLabel: '英文品名', //白底顯示
                            name: 'MMNAME_E',
                            enforceMaxLength: true,
                            maxLength: 80,
                            readOnly: true
                        }, {
                            xtype: 'monthfield',
                            fieldLabel: '月份',
                            name: 'EXP_DATE',
                            id: 'EXP_DATE',
                            enforceMaxLength: true,
                            allowBlank: false, // 欄位為必填
                            fieldCls: 'required',
                            format: 'Xm'
                        }, {
                            xtype: 'displayfield',
                            fieldLabel: '月份', //白底顯示
                            name: 'EXP_DATE_DISPLAY',
                            readOnly: true
                        }, {
                            xtype: 'monthfield',
                            fieldLabel: '警示效期(年/月)',
                            name: 'WARNYM',
                            id: 'WARNYM',
                            enforceMaxLength: true,
                            allowBlank: false, // 欄位是否為必填
                            fieldCls: 'required',
                            format: 'Xm'
                        }, {
                            fieldLabel: '警示效期(年/月)',//白框顯示
                            name: 'WARNYM_TEXT',
                            enforceMaxLength: true,
                            maxLength: 100,
                            readOnly: true,
                        }, {
                            fieldLabel: '藥品批號',
                            name: 'LOT_NO',
                            enforceMaxLength: true,
                            maxLength: 20,
                            readOnly: true,
                            allowBlank: false, // 欄位為必填
                            fieldCls: 'required'
                        }, {
                            xtype: 'displayfield',
                            fieldLabel: '藥品批號', //白底顯示
                            name: 'LOT_NO_DISPLAY',
                            readOnly: true
                        }, {
                            xtype: "numberfield",
                            fieldLabel: '數量',
                            name: 'EXP_QTY',
                            enforceMaxLength: true,
                            maxLength: 15,
                            readOnly: true,
                            allowBlank: false, // 欄位為必填
                            fieldCls: 'required',
                            hideTrigger: true,
                            minValue: 0,
                            negativeText: "數量不可為負數",
                            decimalPrecision: 0,
                            allowDecimals: false,
                            forcePrecision: false
                        }, {
                            xtype: 'textarea',
                            fieldLabel: '備註',
                            name: 'MEMO',
                            enforceMaxLength: true,
                            maxLength: 60,
                            readOnly: true,
                            allowBlank: true
                        }, {
                            xtype: 'combo',
                            store: CloseFlagStore,
                            fieldLabel: '結案否',
                            name: 'CLOSEFLAG',
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            autoSelect: true,
                            multiSelect: false,
                            editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                            matchFieldWidth: true,
                            blankText: "請選擇結案否",
                            allowBlank: false,
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                            readOnly: true,
                            fieldCls: 'required'
                        }, {
                            fieldLabel: '結案否', //紅底顯示
                            name: 'CLOSEFLAG_NAME',
                            enforceMaxLength: true,
                            maxLength: 100,
                            readOnly: true,
                            fieldCls: 'required'
                        }, {
                            xtype: 'displayfield',
                            fieldLabel: 'MAIL狀態', //白底顯示
                            name: 'MAIL_STATUS',
                            readOnly: true
                        }
                    ]
                }
            ],
            buttons: [{
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if ((T3Form.getForm().findField("MMCODE").getValue().trim()) == null ||
                        (T3Form.getForm().findField("MMCODE").getValue().trim()) == "") {
                        Ext.Msg.alert('提醒', "<span style='color:red'>院內碼不可為空</span>，請重新輸入。");
                        msglabel(" <span style='color:red'>院內碼不可為空</span>，請重新輸入。");
                    }
                    else if ((T3Form.getForm().findField("EXP_DATE").getValue()) == null ||
                        (T3Form.getForm().findField("EXP_DATE").getValue()) == "") {
                        Ext.Msg.alert('提醒', "<span style='color:red'>月份不可為空</span>，請重新輸入。");
                        msglabel(" <span style='color:red'>月份不可為空</span>，請重新輸入。");
                    }
                    else if ((T3Form.getForm().findField("LOT_NO").getValue()) == null ||
                        (T3Form.getForm().findField("LOT_NO").getValue()) == "") {
                        Ext.Msg.alert('提醒', "<span style='color:red'>批號不可為空</span>，請重新輸入。");
                        msglabel(" <span style='color:red'>批號不可為空</span>，請重新輸入。");
                    }
                    else if ((T3Form.getForm().findField("EXP_QTY").getValue()) == null ||
                        (T3Form.getForm().findField("EXP_QTY").getValue()) == "") {
                        Ext.Msg.alert('提醒', "<span style='color:red'>數量需大於0</span>，請重新輸入。");
                        msglabel(" <span style='color:red'>數量需大於0</span>，請重新輸入。");
                    } else if ((T3Form.getForm().findField("WARNYM").getValue()) == null ||
                        (T3Form.getForm().findField("WARNYM").getValue()) == "") {
                        Ext.Msg.alert('提醒', "<span style='color:red'>警示效期不可為空</span>，請重新輸入。");
                        msglabel(" <span style='color:red'>警示效期不可為空</span>，請重新輸入。");
                    }
                    else if ((T3Form.getForm().findField("CLOSEFLAG").getValue()) == null ||
                        (T3Form.getForm().findField("CLOSEFLAG").getValue()) == "") {
                        Ext.Msg.alert('提醒', "<span style='color:red'>結案否不可為空</span>，請重新輸入。");
                        msglabel(" <span style='color:red'>結案否不可為空</span>，請重新輸入。");
                    }
                    else {
                        if (this.up('form').getForm().isValid()) { // 檢查T3Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                            var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                            Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                                if (btn === 'yes') {
                                    T3Submit();
                                    Ext.getCmp('changeVender').hide();
                                }
                            });
                        }
                        else {
                            Ext.Msg.alert('提醒', '輸入資料格式有誤');
                            msglabel(" 輸入資料格式有誤");
                        }
                    }
                }
            }, {
                    itemId: 'cancel', text: '取消', hidden: true, handler: function () {
                        T3Cleanup();
                        Ext.getCmp('changeVender').hide();
                        if (isAgenChanged) {
                            T3Load(false);
                        }
                    }
            }]
        });

        // T3 點擊新增/修改之後
        function setFormT3(x, t) {
            viewport.down('#t3Grid').mask();
            viewport.down('#form').setTitle(t + T3Name);
            viewport.down('#form').expand();
            Ext.getCmp('changeVender').hide();
            var f = T3Form.getForm();
            msglabel("");
            if (x === "I") {
                isNew = true;
                var r = Ext.create('WEBAPP.model.AB0053_3'); // /Scripts/app/model/AB0053_3.js
                T3Form.loadRecord(r); // 建立空白model,在新增時載入T3Form以清空欄位內容
                u = f.findField("MMCODE");
                u.clearInvalid();
                u.setReadOnly(false);
                f.findField('EXP_DATE').setReadOnly(false);
                f.findField("EXP_DATE").clearInvalid();
                f.findField("LOT_NO").clearInvalid();
                f.findField("EXP_QTY").clearInvalid();
                f.findField('CLOSEFLAG').clearInvalid();
                setCmpShowCondition(true, false, true, false, true, false, true, false, true, false);
                f.findField('EXP_DATE').setValue(new Date());
                f.findField('CLOSEFLAG').setValue('N');
                Ext.getCmp("mmcodeComboSet_i").getComponent('btnMmcode_i').show();
                f.findField('MAIL_STATUS').setValue('未寄送');
            }
            else {
                u = f.findField('MMCODE');
                setCmpShowCondition(false, true, false, true, false, true, true, false, true, false);
                f.findField('EXP_DATE').setValue(new Date(f.findField('EXP_DATE').getValue()));
                Ext.getCmp("mmcodeComboSet_i").getComponent('btnMmcode_i').hide();
                Ext.getCmp('changeVender').show();
            }

            f.findField('x').setValue(x);
            f.findField('WARNYM').setReadOnly(false);
            f.findField('LOT_NO').setReadOnly(false);
            f.findField('EXP_QTY').setReadOnly(false);
            f.findField('MEMO').setReadOnly(false);
            f.findField('CLOSEFLAG').setReadOnly(false);
            T3Form.down('#cancel').setVisible(true);
            T3Form.down('#submit').setVisible(true);
            u.focus();//指定游標停留在u指定的欄位
        }

        // T3 點擊資料列
        function setFormT3a() {
            T3Grid.down('#edit').setDisabled(false);
            T3Grid.down('#delete').setDisabled(false);

            if (T3LastRec) {
                isNew = false;
                T3Form.loadRecord(T3LastRec);
            }
            else {
                T3Form.getForm().reset();
            }
        }

        // T3 儲存
        function T3Submit() {
            var f = T3Form.getForm();
            if (f.findField("x").getValue() == 'D') {
                f.findField("EXP_DATE").setValue(new Date(f.findField("EXP_DATE").getValue()));
            }
            if (f.isValid()) {
                var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                myMask.show();
                f.submit({
                    url: T3Set,
                    success: function (form, action) {
                        myMask.hide();
                        setCmpShowCondition(false, true, false, true, false, true, false, true, false, true);
                        Ext.getCmp("mmcodeComboSet_i").getComponent('btnMmcode_i').hide();
                        //T3Load();
                        var f2 = T3Form.getForm();
                        var r = f2.getRecord();
                        var mmcodevalue = T3Form.getForm().findField("MMCODE").getValue();
                        var expdatevalue = T3Form.getForm().findField("EXP_DATE").rawValue.substring(0, 5);
                        switch (f2.findField("x").getValue()) {
                            case "I":
                                //var v = action.result.etts[0];
                                //r.set(v);
                                //T3Store.insert(0, r);
                                msglabel(" 資料新增成功");
                                //r.commit();
                                T3Query.getForm().findField("P3").setValue(mmcodevalue);
                                T3Query.getForm().findField("P4").setValue(expdatevalue);
                                break;
                            case "U":
                                //var v = action.result.etts[0];
                                //r.set(v);
                                msglabel(" 資料修改成功");
                                //r.commit();
                                break;
                            case "D":
                                //T3Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                                msglabel(" 資料刪除成功");
                                //r.commit();
                                break;
                        }
                        myMask.hide();
                        viewport.down('#form').setCollapsed("true");
                        T3LastRec = false;
                        if (f2.findField("x").getValue() == 'U') {
                            T3Load(false);
                        } else {
                            T3Load(true);
                        }
                        T3Form.getForm().reset();
                        
                        T3Cleanup();
                    },
                    failure: function (form, action) {
                        myMask.hide();
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
                });
            }
        }

        //T3 CLEANUP
        function T3Cleanup() {
            viewport.down('#t3Grid').unmask();
            var f = T3Form.getForm();
            f.reset();
            f.getFields().each(function (fc) {
                fc.readOnly = true;
                fc.setReadOnly(true);
            });

            T3Form.down('#cancel').hide();
            T3Form.down('#submit').hide();
            viewport.down('#form').setTitle('瀏覽');
            viewport.down('#form').setCollapsed("true");
            setFormT3a();
            setCmpShowCondition(false, true, false, true, false, true, false, true, false, true);
            Ext.getCmp("mmcodeComboSet_i").getComponent('btnMmcode_i').hide();
        }

        //發送EMAIL
        function SendMail() {
            Ext.Ajax.request({
                url: T3Set,
                method: reqVal_p,
                success: function (response) {
                    T3Load(true);
                    Ext.Msg.alert('提醒', '寄送成功');
                    msglabel("寄送成功");
                },
                failure: function (response, options) {
                }
            });
        }

        function SendMail_v2(list) {
            Ext.Ajax.request({
                url: '/api/AB0053/SendMail_v2',
                method: reqVal_p,
                params: { itemString: Ext.util.JSON.encode(list) },
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        T3Load(true);
                        Ext.Msg.alert('提醒', '寄送成功');
                        msglabel("寄送成功");
                    } else {
                        Ext.Msg.alert('提醒', data.msg);
                    }
                    
                },
                failure: function (response, options) {
                }
            });
        }

        //複製資料
        function CopyData() {
            Ext.Ajax.request({
                url: T3Set,
                method: reqVal_p,
                success: function (response, a, b) {
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        T3Load(true);
                        Ext.Msg.alert('提醒', '資料複製成功');
                        msglabel("資料複製成功");
                    }
                    else {
                        if (data.msg.indexOf('ORA-00001', 0) < 0) {
                            msglabel(" 資料複製失敗: " + data.msg);
                            Ext.Msg.alert('提醒', " 資料複製失敗: " + data.msg);
                        }
                        else {
                            msglabel(" 資料複製失敗: 有院內碼及批號相同，其餘欄位卻不一致的資料，請先行處理");
                            Ext.Msg.alert('提醒', " 資料複製失敗: 有院內碼及批號相同，其餘欄位卻不一致的資料，請先行處理");
                        }
                    }
                },
                failure: function (response, options) {
                }
            });
        }
    }

    //#region 2020-07-09 新增: 修改廠商window

    var isAgenChanged = false;

    Ext.define('T4Model', {
        extend: 'Ext.data.Model',
        fields: ['AGEN_NO', 'AGEN_NAMEC'],
    });

    //  T4 QUERY
    var T4Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 50,
            width: 210
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP4',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'radiogroup',
                    name: 'P0',
                    fieldLabel: '類別',
                    id: 'VENDER_QUERY',
                    width:300,
                    items: [
                        { boxLabel: 'HIS曾經的廠商', name: 'VENDER_QUERY', inputValue: 'H', width:100, checked: true },
                        { boxLabel: '廠商基本資料', name: 'VENDER_QUERY', inputValue: 'P', width: 100 }
                    ]
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        msglabel("");
                        T4Load();
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        msglabel("");
                        f.findField('VENDER_QUERY').setValue({ VENDER_QUERY: 'A'});
                    }
                }
            ]
        }]
    });

    //T4 Load
    function T4Load() {
        T4Tool.moveFirst();
    }

    // T4 STORE
    var T4Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T4Model',
        pageSize: 999999,
        remoteSort: true,
        sorters: [{ property: 'AGEN_NO', direction: 'ASC' }, { property: 'AGEN_NAMEC', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T4Query.getForm().findField('P0').getValue()['VENDER_QUERY'],
                    p1: T3LastRec.data.MMCODE
                };
                Ext.apply(store.proxy.extraParams, np);
            }

        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0053/VenderInfos',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    // T4 TOOLBAR
    var T4Tool = Ext.create('Ext.PagingToolbar', {
        store: T4Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                id: 'selectVender',
                text: '選取',
                handler: function () {
                    var selection = T4Grid.getSelection();
                    var temp = selection[0].data;

                    Ext.Ajax.request({
                        url: '/api/AB0053/UpdateAgenno',
                        method: reqVal_p,
                        params: {
                            agen_no : temp.AGEN_NO,
                            agen_namec:  temp.AGEN_NAMEC,
                            mmcode: T3LastRec.data.MMCODE,
                            exp_date: T3LastRec.data.EXP_DATE,
                            lot_no: T3LastRec.data.LOT_NO,
                            source: T4Query.getForm().findField('P0').getValue()['VENDER_QUERY']
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel("廠商修改成功");
                                T3Form.getForm().findField('AGEN_NAMEC').setValue(temp.AGEN_NO + ' ' + temp.AGEN_NAMEC);
                                venderWindow.hide();

                                isAgenChanged = true;
                            }
                        },
                        failure: function (response, options) {

                        }
                    });
                }
        }]
    });

    // T4 GRID查詢結果
    var T4Grid = Ext.create('Ext.grid.Panel', {
        store: T4Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: windowHeight,
        selModel: {
            mode: 'SINGLE'
        },
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T4Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T4Tool]
        }],
        columns: [{
            xtype: 'rownumberer',
            width: 70,
            align: 'Center',
            labelAlign: 'Center'
        }, {
            dataIndex: 'AGEN_NO',
            text: "廠商代碼",
            width: 80
        },
        {
            text: "廠商名稱",
            dataIndex: 'AGEN_NAMEC',
            width: 150
        }],
        listeners: {
            
            selectionchange: function (model, records) {
                Ext.getCmp('selectVender').disable();

                if (records.length > 0) {
                    Ext.getCmp('selectVender').enable();
                }
            }
        }
    });

    var venderWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        items: [T4Grid],
        modal: true,
        width: windowWidth,
        height: windowHeight,
        resizable: true,
        draggable: true,
        closable: false,
        y: 0,
        buttons: [{
            text: '關閉',
            handler: function () {
                venderWindow.hide();
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                //Ext.getCmp('transferT31').disable();
                T4Store.removeAll();
                Ext.getCmp('selectVender').disable();
                venderWindow.setY(0);
            }
        }
    });
    venderWindow.hide();
    //#endregion

    //#endregion

    //#region 定義TAB內容
    //＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ 定義TAB內容 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        var TATabs = Ext.widget('tabpanel', {
            listeners: {
                tabchange: function (tabpanel, newCard, oldCard) {
                    switch (newCard.title) {
                        case "各庫效期表":
                            T1Form.setVisible(true);
                            T2Form.setVisible(false);
                            T3Form.setVisible(false);
                            T1Query.getForm().findField('P0').focus();
                            break;
                        case "效期總表":
                            T1Form.setVisible(false);
                            T2Form.setVisible(true);
                            T3Form.setVisible(false);
                            T2Query.getForm().findField('P2').focus();
                            T1Query.getForm().findField('P0').clearInvalid();
                            break;
                        case "藥庫專區":
                            T1Form.setVisible(false);
                            T2Form.setVisible(false);
                            T3Form.setVisible(true);
                            T3Query.getForm().findField('P3').focus();
                            T1Query.getForm().findField('P0').clearInvalid();
                            break;
                    }
                }
            },
            layout: 'fit',
            plain: true,
            border: false,
            resizeTabs: true,       //改變tab尺寸       
            enableTabScroll: true,  //是否允許Tab溢出時可以滾動
            defaults: {
                // autoScroll: true,
                closabel: false,    //tab是否可關閉
                padding: 0,
                split: true
            },
            items: [{
                itemId: 't1Grid',
                title: '各庫效期表',
                layout: 'border',
                padding: 0,
                split: true,
                region: 'center',
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T1Grid]
            }, {
                itemId: 't2Grid',
                title: '效期總表',
                layout: 'border',
                padding: 0,
                split: true,
                region: 'center',
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T2Grid]
            }, {
                itemId: 't3Grid',
                title: '藥庫專區',
                layout: 'border',
                padding: 0,
                split: true,
                region: 'center',
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T3Grid]
            }]
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
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 330,
                title: '瀏覽',
                collapsed: true,
                border: false,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T1Form, T2Form, T3Form]
            }, {
                itemId: 't1Form',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [TATabs]
            }
            ]
        });

        function showComponent(form, fieldName) {
            var u = form.findField(fieldName);
            u.show();
        }

        function hideComponent(form, fieldName) {
            var u = form.findField(fieldName);
            u.hide();
        }

        //控制不可更改項目的顯示
        function setCmpShowCondition(MMCODE, MMCODE_DISPLAY, EXP_DATE, EXP_DATE_DISPLAY, LOT_NO, LOT_NO_DISPLAY, WARNYM, WARNYM_TEXT, CLOSEFLAG, CLOSEFLAG_NAME) {
            var f = T3Form.getForm();
            MMCODE ? showComponent(f, "MMCODE") : hideComponent(f, "MMCODE");
            MMCODE_DISPLAY ? showComponent(f, "MMCODE_DISPLAY") : hideComponent(f, "MMCODE_DISPLAY");
            EXP_DATE ? showComponent(f, "EXP_DATE") : hideComponent(f, "EXP_DATE");
            EXP_DATE_DISPLAY ? showComponent(f, "EXP_DATE_DISPLAY") : hideComponent(f, "EXP_DATE_DISPLAY");
            LOT_NO ? showComponent(f, "LOT_NO") : hideComponent(f, "LOT_NO");
            LOT_NO_DISPLAY ? showComponent(f, "LOT_NO_DISPLAY") : hideComponent(f, "LOT_NO_DISPLAY");
            WARNYM ? showComponent(f, "WARNYM") : hideComponent(f, "WARNYM");
            WARNYM_TEXT ? showComponent(f, "WARNYM_TEXT") : hideComponent(f, "WARNYM_TEXT");
            CLOSEFLAG ? showComponent(f, "CLOSEFLAG") : hideComponent(f, "CLOSEFLAG");
            CLOSEFLAG_NAME ? showComponent(f, "CLOSEFLAG_NAME") : hideComponent(f, "CLOSEFLAG_NAME");
        }
    }
    //#endregion

    //#region load
    //＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ LOAD ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        function T1Load() {
            T1Tool.moveFirst();
        }
        function T2Load() {
            T2Tool.moveFirst();
        }
        function T3Load(isMoveFirst) {
            var f = T3Query.getForm();
            if (f.findField('P5').getValue() == '' || f.findField('P5').getValue() == null) {
                Ext.Msg.alert('提醒', '<span style="color:red">結案否</span>為必填');
                return;
            }
            if (f.findField('P6').getValue() == '' || f.findField('P6').getValue() == null) {
                Ext.Msg.alert('提醒', '<span style="color:red">Mail狀態</span>為必填');
                return;
            }
            if (isMoveFirst) {
                T3Tool.moveFirst();
            } else {
                T3Store.load({
                    params: {
                        start: 0
                    }
                });
            }
            isAgenChanged = false;
        }
    }
    //#endregion

    //#region initialize
    //＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ 起始執行 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    setDefaultWhno();
    setCloseFlagCombo();
    setMailStatusQueryCombo();

    T3Query.getForm().findField('P5').setValue(' ');
    T3Query.getForm().findField('P6').setValue(' ');

    setCmpShowCondition(false, true, false, true, false, true, false, true, false, true);
    Ext.getCmp("mmcodeComboSet_i").getComponent('btnMmcode_i').hide();
    //#endregion
});
