Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    //var T1Name = "緊急醫療出貨申請";

    var T1Rec = 0;
    var T1LastRec = null;

    //院內碼
    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P0',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0109/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        //getDefaultParams: function () { //查詢時Controller固定會收到的參數
        //    return {
        //        p1: '01'
        //    };
        //},
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });
    //入庫庫房Store
    var st_TOWH = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0109/GetTOWHcombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });
    //狀態Store
    var st_STATUS = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0109/GetStatusCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    function setMmcode(args) {
        var f = T1Form.getForm();
        if (args.MMCODE !== '') {
            f.findField("MMCODE").setValue(args.MMCODE);
            T1FormMMCode.doQuery();
            var func = function () {
                var record = T1FormMMCode.store.getAt(0);
                T1FormMMCode.select(record);
                T1FormMMCode.fireEvent('select', this, record);
                T1FormMMCode.store.un('load', func);
            };
            T1FormMMCode.store.on('load', func);
        }
    }
    //T1Form 院內碼
    var T1FormMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        readOnly: true,
        allowBlank: false,
        fieldCls: 'required',
        width: 220,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0109/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT', 'CR_UPRICE', 'M_PAYKIND', 'AGEN_NAME',
            'AGEN_NO', 'EMAIL', 'M_CONTPRICE', 'UPRICE', 'M_APPLYID', 'M_CONTID'], //查詢完會回傳的欄位
        //getDefaultParams: function () { //查詢時Controller固定會收到的參數
        //    return {
        //        p1: T1Form.getForm().findField('MAT_CLASS').getValue()
        //    };
        //},
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));   
                T1Form.getForm().findField('MMNAME_C').setValue(r.get('MMNAME_C'));
                T1Form.getForm().findField('MMNAME_E').setValue(r.get('MMNAME_E'));
                T1Form.getForm().findField('BASE_UNIT').setValue(r.get('BASE_UNIT'));
                T1Form.getForm().findField('CR_UPRICE').setValue(r.get('CR_UPRICE'));
                T1Form.getForm().findField('M_PAYKIND').setValue(r.get('M_PAYKIND'));
                T1Form.getForm().findField("AGEN_NAME").setValue(r.get('AGEN_NAME'));
                T1Form.getForm().findField("AGEN_NO").setValue(r.get('AGEN_NO'));
                T1Form.getForm().findField("EMAIL").setValue(r.get('EMAIL'));
                T1Form.getForm().findField("M_CONTPRICE").setValue(r.get('M_CONTPRICE'));
                T1Form.getForm().findField("UPRICE").setValue(r.get('UPRICE'));
                T1Form.getForm().findField("M_APPLYID").setValue(r.get('M_APPLYID'));
                T1Form.getForm().findField("M_CONTID").setValue(r.get('M_CONTID'));
                

                T1Form.getForm().findField('UseWhereDisplay').setValue('');
                T1Form.getForm().findField('UseWhenDisplay').setValue('');
                T1Form.getForm().findField('TelDisplay').setValue('');
                //st_lotno.load();
                //if ((r.get('M_APPLYID') == 'E') && (r.get('M_CONTID') == '3')) {
                     //T1Form.getForm().findField("USEWHERE").setReadOnly(false);
                    //T1Form.getForm().findField("USEWHEN").setReadOnly(false);
                    //T1Form.getForm().findField("TEL").setReadOnly(false);
                //}
                //else {
                    //T1Form.getForm().findField("USEWHERE").setReadOnly(true);
                    //T1Form.getForm().findField("USEWHEN").setReadOnly(true);
                    //T1Form.getForm().findField("TEL").setReadOnly(true);
           
                //}
                
                if ((r.get('M_APPLYID') == 'E') && (r.get('M_CONTID') == '3')) {
                    T1Form.getForm().findField('USEWHERE').setReadOnly(false); //用途
                    T1Form.getForm().findField('USEWHEN').setReadOnly(false);  //本次申請量,預估使用時間
                    T1Form.getForm().findField('TEL').setReadOnly(false);      //電話

                    T1Form.getForm().findField('USEWHERE').show();
                    T1Form.getForm().findField('USEWHEN').show();
                    T1Form.getForm().findField('TEL').show();

                    T1Form.getForm().findField('UseWhereDisplay').hide();
                    T1Form.getForm().findField('UseWhenDisplay').hide();
                    T1Form.getForm().findField('TelDisplay').hide();
                }
                else {
                    T1Form.getForm().findField('USEWHERE').setReadOnly(true); //用途
                    T1Form.getForm().findField('USEWHEN').setReadOnly(true);  //本次申請量,預估使用時間
                    T1Form.getForm().findField('TEL').setReadOnly(true);      //電話

                    T1Form.getForm().findField('USEWHERE').hide();
                    T1Form.getForm().findField('USEWHEN').hide();
                    T1Form.getForm().findField('TEL').hide();

                    T1Form.getForm().findField('UseWhereDisplay').show();
                    T1Form.getForm().findField('UseWhenDisplay').show();
                    T1Form.getForm().findField('TelDisplay').show();
                }
            }
        }
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'CRDOCNO', type: 'string' },        // 緊急醫療出貨單編號
            { name: 'MMCODE', type: 'string' },         // 院內碼
            { name: 'MMNAME_C', type: 'string' },       // 中文品名
            { name: 'MMNAME_E', type: 'string' },       // 英文品名  
            { name: 'APPQTY', type: 'string' },         // 申請數量
            { name: 'BASE_UNIT', type: 'string' },      // 計量單位(包裝單位)
            { name: 'TOWH', type: 'string' },           // 入庫庫房
            { name: 'REQDATE', type: 'string' },        // 要求到貨日期
            { name: 'DRNAME', type: 'string' },         // 使用醫師
            { name: 'PATIENTNAME', type: 'string' },    // 病人姓名
            { name: 'CHARTNO', type: 'string' },        // 病人病歷號
            { name: 'CR_UPRICE', type: 'string' },      // 單價
            { name: 'M_PAYKIND', type: 'string' },      // 收費屬性
            { name: 'AGEN_NAME', type: 'string' },      // 廠商名稱
            { name: 'INID', type: 'string' },           // 庫房責任中心
            { name: 'APPID', type: 'string' },          // 申請人ID
            { name: 'APPNM', type: 'string' },          // 申請人名稱
            { name: 'APPTIME', type: 'string' },        // 申請時間
            { name: 'EMAIL', type: 'string' },          // 廠商Email
            { name: 'CREATE_USER', type: 'string' },    // 建立人員
            { name: 'CREATE_TIME', type: 'string' },    // 建立時間
            { name: 'CR_STATUS', type: 'string' },      // 狀態
            { name: 'WH_NAME', type: 'string' },        // 庫房名稱
            { name: 'AGEN_NO', type: 'string' },        // 廠商代碼
            { name: 'STATUS', type: 'string' },         // 狀態(中文)
            { name: 'USEWHERE', type: 'string' },       // 用途
            { name: 'USEWHEN', type: 'string' },        // 本次申請量預估使用時間
            { name: 'TEL', type: 'string' },            // 電話
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'CRDOCNO', direction: 'ASC' }],//依CRDOCNO排序
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0109/GetAll',  // 呼叫AB0109Control 中的GetAll,再至DB取得資料值 
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: { //觸發
            beforeload: function (store, options) {
                var np = { //前端取得參數
                    p0: T1Query.getForm().findField('P0').getValue(),  //MMCODE
                    p1: T1Query.getForm().findField('P1').getValue(),  //TOWH
                    p2: T1Query.getForm().findField('P2').getValue(),  //CR_STATUS
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        border: false,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 95
        },
        items: [
            T1QueryMMCode, //院內碼
            {
                xtype: 'combo',
                fieldLabel: '入庫庫房',
                store: st_TOWH,
                id: 'P1',
                name: 'P1',
                labelWidth: 80,
                width: 240,
                queryMode: 'local',
                //fieldCls: 'required',
                //allowBlank: false,
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>' //前端格式化可折行
            }, {
                xtype: 'combo',
                fieldLabel: '狀態',
                store: st_STATUS,
                id: 'P2',
                name: 'P2',
                labelWidth: 80,
                width: 240,
                queryMode: 'local',
                //fieldCls: 'required',
                //allowBlank: false,
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
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
                    f.findField('P0').focus(); //進入畫面時輸入游標預設在MMCODE
                    msglabel('');
                }
            }
        ]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true, //列示資料筆數
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增',
                handler: function () {
                    T1Set = '/api/AB0109/Create'; // AB0109Controller的Create
                    msglabel('');
                    setFormT1('I', '新增');
                }
            }, {
                itemId: 'edit', text: '修改', disabled: true,
                handler: function () {
                    T1Set = '/api/AB0109/Update';
                    msglabel('');
                    setFormT1("U", '修改');
                }
            }, {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AB0109/Delete';
                            T1Form.getForm().findField('x').setValue('D'); //設定x的值為D
                            T1Submit();
                        }
                    }
                    );
                }
            }, {
                itemId: 'apply', text: '申請', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('申請', '是否確定申請？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AB0109/Apply';
                            T1Form.getForm().findField('x').setValue('A'); //設定x的值為A
                            T1Submit();
                        }
                    })

                }

            }, {
                itemId: 'reject', text: '撤銷', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('撤銷', '是否確定撤銷？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AB0109/Reject';
                            T1Form.getForm().findField('x').setValue('R'); //設定x的值為R
                            T1Submit();
                        }
                    })
                }
            }
        ]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', { //表格列
        //title: T1Name,
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
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "緊急醫療出貨單編號",
            dataIndex: 'CRDOCNO',
            width: 130
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 130
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 130
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 130
        }, {
            text: "申請數量",
            dataIndex: 'APPQTY',
            width: 65
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            width: 90
        }, {
            text: "入庫庫房",
            dataIndex: 'WH_NAME',
            width: 130
        }, {
            text: "要求到貨日期",
            dataIndex: 'REQDATE',
            width: 90
        }, {
            text: "使用醫師",
            dataIndex: 'DRNAME',
            width: 100
        }, {
            text: "病人姓名",
            dataIndex: 'PATIENTNAME',
            width: 100
        }, {
            text: "病人病歷號",
            dataIndex: 'CHARTNO',
            width: 130
        }, {
            text: "單價",
            dataIndex: 'CR_UPRICE',
            width: 90
        }, {
            text: "收費屬性",
            dataIndex: 'M_PAYKIND',
            width: 130
        }, {
            text: "廠商名稱",
            dataIndex: 'AGEN_NAME',
            width: 130
        }, {
            text: "庫房責任中心",
            dataIndex: 'INID',
            width: 130
        }, {
            text: "申請人",
            dataIndex: 'APPNM',
            width: 100
        }, {
            text: "申請時間",
            dataIndex: 'APPTIME',
            width: 100
        }, {
            text: "廠商Email",
            dataIndex: 'EMAIL',
            width: 130
        }, {
            text: "建立人員",
            dataIndex: 'CREATE_USER',
            width: 130
        }, {
            text: "建立時間",
            dataIndex: 'CREATE_TIME',
            width: 130
        }, {
            text: "狀態",
            dataIndex: 'STATUS', //狀態（中文)
            width: 130
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0]; //將游標所在資料寫至T1LastRec
                setFormT1a();  //T1 Grid資料被點選時的動作
            }
        }
    });

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'vbox',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [
            {
                name: 'x',
                xtype: 'hidden'
            }, {
                xtype: 'displayfield',
                fieldLabel: '緊急醫療出貨單編號',
                name: 'CRDOCNO',
                submitValue: true, //跟著文字一塊送後端
            }, {
                xtype: 'container',
                layout: 'hbox',
                //padding: '0 7 7 0',
                items: [
                    T1FormMMCode,
                    {
                        xtype: 'button',
                        itemId: 'btnMmcode',
                        iconCls: 'TRASearch', //放大鏡
                        handler: function () {
                            var f = T1Form.getForm();
                            if (!f.findField("MMCODE").readOnly) {
                                popMmcodeForm(viewport, '/api/AB0109/GetMmcode', {
                                    MMCODE: f.findField("MMCODE").getValue()
                                }, setMmcode);
                            }
                        }
                    },

                ]
            }, {
                xtype: 'displayfield',
                fieldLabel: '院內碼',
                name: 'MmcodeDisplay',
                hidden: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '中文品名',
                name: 'MMNAME_C',
                submitValue: true, //跟著文字一塊送後端
            }, {
                xtype: 'displayfield',
                fieldLabel: '英文品名',
                name: 'MMNAME_E',
                submitValue: true, //跟著文字一塊送後端
            }, {
                xtype: 'displayfield',
                fieldLabel: '計量單位',
                name: 'BASE_UNIT',
                submitValue: true, //跟著文字一塊送後端
            }, {
                fieldLabel: '申請數量',
                name: 'APPQTY',
                enforceMaxLength: true,
                maxLength: 20,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                maskRe: /[0-9]/, //不可輸入小數
                fieldCls: 'required'
            }, {
                xtype: 'displayfield',
                fieldLabel: '申請數量',
                name: 'AppQtyDisplay',
                hidden: true
            }, {
                xtype: 'combo',
                fieldLabel: '入庫庫房',
                name: 'TOWH',
                store: st_TOWH,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                anyMatch: true,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                fieldCls: 'required',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
            }, {
                xtype: 'displayfield',
                fieldLabel: '入庫庫房',
                name: 'WH_NAME',
                hidden: true
            }, {
                xtype: 'datefield',
                fieldLabel: '要求到貨日期',
                name: 'REQDATE',
                allowBlank: false, // 欄位為必填
                fieldCls: 'required',
                readOnly: true,
            }, {
                xtype: 'displayfield',
                fieldLabel: '要求到貨日期',
                name: 'ReqDateDisplay',
                hidden: true
            }, {
                fieldLabel: '使用醫師',
                name: 'DRNAME',
                enforceMaxLength: true,
                maxLength: 20,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '使用醫師',
                name: 'DrnameDisplay',
                hidden: true
            }, {
                fieldLabel: '病人姓名',
                name: 'PATIENTNAME',
                enforceMaxLength: true,
                maxLength: 20,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                fieldCls: 'required'
            }, {
                xtype: 'displayfield',
                fieldLabel: '病人姓名',
                name: 'PatientNameDisplay',
                hidden: true
            }, {
                fieldLabel: '病人病歷號',
                name: 'CHARTNO',
                enforceMaxLength: true,
                maxLength: 20,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                fieldCls: 'required',
            }, {
                xtype: 'displayfield',
                fieldLabel: '病人病歷號',
                name: 'ChartNoDisplay',
                hidden: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '單價',
                name: 'CR_UPRICE',
                submitValue: true, //跟著文字一塊送後端
            }, {
                xtype: 'displayfield',
                fieldLabel: '收費屬性',
                name: 'M_PAYKIND',
                submitValue: true, //跟著文字一塊送後端
            }, {
                xtype: 'displayfield',
                fieldLabel: '廠商名稱',
                name: 'AGEN_NAME',
            }, {
                xtype: 'textarea',
                fieldLabel: '用途',
                name: 'USEWHERE',
                enforceMaxLength: true,
                maxLength: 100,   
            }, {
                fieldLabel: '本次申請量<br>預估使用時間',
                name: 'USEWHEN',
                enforceMaxLength: true,
                maxLength: 100,
            }, {
                fieldLabel: '電話',
                name: 'TEL',
                enforceMaxLength: true,
                maxLength: 20,
            }, {
                xtype: 'displayfield',
                fieldLabel: '用途',
                name: 'UseWhereDisplay',
                hidden: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '本次申請量<br>預估使用時間',
                name: 'UseWhenDisplay',
                hidden: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '電話',
                name: 'TelDisplay',
                hidden: true
            }, {
                name: 'AGEN_NO',
                xtype: 'hidden'
            }, {
                name: 'WH_NAME',
                xtype: 'hidden'
            }, {
                name: 'M_CONTPRICE',
                xtype: 'hidden'
            }, {
                name: 'EMAIL',
                xtype: 'hidden'
            }, {
                name: 'UPRICE',
                xtype: 'hidden'
            }, {
                name: 'CR_STATUS',
                xtype: 'hidden'
            }, {
                name: 'M_APPLYID',
                xtype: 'hidden'
            }, {
                name: 'M_CONTID',
                xtype: 'hidden'
            }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                var isSub = true;
                if ((this.up('form').getForm().findField('M_APPLYID').getValue() == 'E') &&
                    (this.up('form').getForm().findField('M_CONTID').getValue() == '3')) {
                    if ((this.up('form').getForm().findField('USEWHERE').getValue() == '') ||
                        (this.up('form').getForm().findField('USEWHEN').getValue() == '') ||
                        (this.up('form').getForm().findField('TEL').getValue() == '')) {
                        Ext.Msg.alert('提醒', '申請申購識別碼=E 且為零購<br>用途、本次申請量預估使用時間、電話 等三個欄位必填');
                        isSub = false;
                    }
                }
                if (this.up('form').getForm().isValid()) {// 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)                 
                    if (this.up('form').getForm().findField('APPQTY').getValue() == '0') {
                        Ext.Msg.alert('提醒', '申請數量不可為0');
                        isSub = false;
                    }                    
                    
                    if (Ext.Date.format(this.up('form').getForm().findField('REQDATE').getValue(), "Ymd") < Ext.Date.format(new Date(), "Ymd")) {
                        Ext.Msg.alert('提醒', '要求到貨日期須 >= 今日');
                        isSub = false;
                    }                    
                    if (isSub) {
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
                            }
                        }
                        );
                    }
                }
                else
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true,
            handler: T1Cleanup
        }]
    });

    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        //viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        cr_status = f.findField('CR_STATUS').getValue();
        //m_applyid = f.findField('M_APPLYID').getValue();
        //m_contid = f.findField('M_CONTID').getValue();
        f.findField('x').setValue(x);
        
        if (x === "I") { //新增
            isNew = true;
            var r = Ext.create('T1Model');
             //使用Chrome時按F12 可中斷程式並進行trace
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("MMCODE"); // 院內碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();
            f.findField('CRDOCNO').setValue('系統自編');
            f.findField('REQDATE').setValue(new Date);
            setDisplayFiled(false);
            u.focus();
        }
        else {
            if ((cr_status == "A") || (cr_status == "B")) {
                setDisplayFiled(false);
            }
            else {
                if ((cr_status != "C")) {
                    f.findField('DRNAME').setReadOnly(false);       //使用醫師
                    f.findField('PATIENTNAME').setReadOnly(false);  //病人姓名
                    f.findField('CHARTNO').setReadOnly(false);      //病人病歷號 

                    f.findField('MmcodeDisplay').show();
                    f.findField('AppQtyDisplay').show();
                    f.findField('WH_NAME').show();
                    f.findField('ReqDateDisplay').show();
                    f.findField('DRNAME').show();
                    f.findField('PATIENTNAME').show();
                    f.findField('CHARTNO').show();

                    f.findField('APPQTY').hide();
                    f.findField('TOWH').hide();
                    f.findField('REQDATE').hide();
                    f.findField('DrnameDisplay').hide();
                    f.findField('PatientNameDisplay').hide();
                    f.findField('ChartNoDisplay').hide();
                }
            }
            f.findField('MMCODE').hide();
            T1Form.down('#btnMmcode').setVisible(false);
            f.findField('MmcodeDisplay').show();
        }
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
    }

    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#delete').setDisabled(T1Rec === 0);
        T1Grid.down('#apply').setDisabled(true);
        T1Grid.down('#reject').setDisabled(true);

        viewport.down('#form').expand(); //將Form展出
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec); //將資料讀入Form
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('MMCODE');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            cr_status = f.findField('CR_STATUS').getValue();
            switch (cr_status) {
                case "A": //待申請
                    T1Grid.down('#apply').setDisabled(false);
                    T1Grid.down('#delete').setDisabled(false);
                    break;
                case "B": //已申請 
                    T1Grid.down('#reject').setDisabled(false);
                    T1Grid.down('#edit').setDisabled(true);
                    T1Grid.down('#delete').setDisabled(true);
                    break;
                case "C": //刪除
                    T1Grid.down('#edit').setDisabled(true);
                    T1Grid.down('#delete').setDisabled(true);
                    break;
                case "D": //撤銷                    
                    T1Grid.down('#edit').setDisabled(false);
                    T1Grid.down('#delete').setDisabled(true);
                    break;
            }
            setDisplayFiled(true);
        }
        else {
            T1Form.getForm().reset();
        }
    }

    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "combo" || fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        setFormT1a();
    }

    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();                    
                    
                    switch (f2.findField("x").getValue()) {                   
                        case "I":                                                        
                            T1Query.getForm().findField('P0').setValue(f2.findField('MMCODE').getValue());
                            T1Query.getForm().findField('P1').setValue(f2.findField('TOWH').getValue());
                            T1Query.getForm().findField('P2').setValue('A');
                            T1Load(false);
                            msglabel('資料新增成功');
                            break;
                        case "U":                                              
                            T1Query.getForm().findField('P0').setValue(f2.findField('MMCODE').getValue());
                            T1Query.getForm().findField('P1').setValue(f2.findField('TOWH').getValue());
                            T1Query.getForm().findField('P2').setValue(f2.findField('CR_STATUS').getValue());
                            T1Load(false);
                            msglabel('資料修改成功');
                            break;
                        case "D":
                            T1Load(false);
                            msglabel('資料刪除成功');
                            break;
                        case "A":
                            T1Load(false);
                            msglabel('資料申請成功');
                            break;
                        case "R":
                            T1Load(false);
                            msglabel('資料撤銷成功');
                            break;
                    }
                    T1Cleanup();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }

    function T1Load(moveFirst) {
        if (moveFirst) {
            T1Tool.moveFirst(); //移動到第一頁，與按一下“first”按鈕有相同的效果。
        }
        else {
            T1Store.load({
                params: {
                    start: 0 //start: 0 從第0筆開始顯示,如果要從後端控制每個分頁起始, 可從這邊傳給後端
                }
            });
        }
        setDisplayFiled(true);
    }

    function setDisplayFiled(DisplayFieldShow) {
        
        var f = T1Form.getForm();
        m_applyid = f.findField('M_APPLYID').getValue();
        m_contid = f.findField('M_CONTID').getValue();
        if (DisplayFieldShow) {
            f.findField('APPQTY').setReadOnly(true);      //申請數量
            f.findField('TOWH').setReadOnly(true);        //入庫庫房   
            f.findField('REQDATE').setReadOnly(true);     //要求到貨日期
            f.findField('DRNAME').setReadOnly(true);      //使用醫師 
            f.findField('PATIENTNAME').setReadOnly(true); //病人姓名
            f.findField('CHARTNO').setReadOnly(true);     //病人病歷號
            f.findField('USEWHERE').setReadOnly(true); //用途
            f.findField('USEWHEN').setReadOnly(true);  //本次申請量,預估使用時間
            f.findField('TEL').setReadOnly(true);      //電話

            f.findField('AppQtyDisplay').show();
            f.findField('WH_NAME').show();
            f.findField('ReqDateDisplay').show();
            f.findField('PatientNameDisplay').show();
            f.findField('ChartNoDisplay').show();
            f.findField('MmcodeDisplay').show();
            f.findField('UseWhereDisplay').show();
            f.findField('UseWhenDisplay').show();
            f.findField('TelDisplay').show();
            f.findField('DrnameDisplay').show();


            f.findField('MMCODE').hide();
            T1Form.down('#btnMmcode').setVisible(false);
            f.findField('APPQTY').hide();
            f.findField('TOWH').hide();
            f.findField('REQDATE').hide();
            f.findField('DRNAME').hide();
            f.findField('PATIENTNAME').hide();
            f.findField('CHARTNO').hide();
            f.findField('USEWHERE').hide();
            f.findField('USEWHEN').hide();
            f.findField('TEL').hide();
        }
        else {
            f.findField('APPQTY').setReadOnly(false);       //申請數量
            f.findField('TOWH').setReadOnly(false);         //入庫庫房 
            f.findField('REQDATE').setReadOnly(false);      //要求到貨日期 
            f.findField('DRNAME').setReadOnly(false);       //使用醫師
            f.findField('PATIENTNAME').setReadOnly(false);  //病人姓名
            f.findField('CHARTNO').setReadOnly(false);      //病人病歷號  

            f.findField('AppQtyDisplay').hide();
            f.findField('WH_NAME').hide();
            f.findField('ReqDateDisplay').hide();
            f.findField('PatientNameDisplay').hide();
            f.findField('ChartNoDisplay').hide();
            f.findField('MmcodeDisplay').hide();
            f.findField('DrnameDisplay').hide();

            f.findField('MMCODE').show();
            T1Form.down('#btnMmcode').setVisible(true);
            f.findField('APPQTY').show();
            f.findField('TOWH').show();
            f.findField('REQDATE').show();
            f.findField('DRNAME').show();
            f.findField('PATIENTNAME').show();
            f.findField('CHARTNO').show();
            
            if ((m_applyid == "E") && (m_contid == "3")) {
                f.findField('USEWHERE').setReadOnly(false); //用途
                f.findField('USEWHEN').setReadOnly(false);  //本次申請量,預估使用時間
                f.findField('TEL').setReadOnly(false);      //電話

                f.findField('USEWHERE').show();
                f.findField('USEWHEN').show();
                f.findField('TEL').show();

                f.findField('UseWhereDisplay').hide();
                f.findField('UseWhenDisplay').hide();
                f.findField('TelDisplay').hide();
            }
            else {
                f.findField('USEWHERE').setReadOnly(true); //用途
                f.findField('USEWHEN').setReadOnly(true);  //本次申請量,預估使用時間
                f.findField('TEL').setReadOnly(true);      //電話

                f.findField('USEWHERE').hide();
                f.findField('USEWHEN').hide();
                f.findField('TEL').hide();

                f.findField('UseWhereDisplay').show();
                f.findField('UseWhenDisplay').show();
                f.findField('TelDisplay').show();
            }
        }
    }

    var viewport = Ext.create('Ext.Viewport', { //前端畫面佈局
        renderTo: body, //始終渲染在頁面
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
            collapsible: false, //是否可以收縮
            title: '',
            border: false,
            items: [T1Grid]
        }, {
            itemId: 'form',
            region: 'east',
            collapsible: true,//是否可以收縮
            floatable: true,
            width: 300,
            title: '',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch' //各子組件的寬度拉伸至與容器的寬度相等.
            },
            items: [T1Form]
        }]
    });

});
